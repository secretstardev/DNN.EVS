#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.Drawing;
using System.Collections.Specialized;
using System.Globalization;

namespace SQLAzureMWUtils
{
    public enum DatabaseObjectsTypes
    {
        Assemblies,
        PartitionFunctions,
        PartitionSchemes,
        Roles,
        Schemas,
        Triggers,
        UserDefinedDataTypes,
        UserDefinedTableTypes,
        XMLSchemaCollections,
        Synonyms
    }

    public class SourceProcessor
    {
        private SMOScriptOptions _smoScriptOpts;
        private AsyncUpdateStatus _updateStatus;
        private AsyncNotificationEventArgs _eventArgs;
        private ScriptDatabase _sdb;
        private string _outputDir;

        public void Initialize(ScriptDatabase sdb, SMOScriptOptions options, AsyncUpdateStatus updateStatus, AsyncNotificationEventArgs eventArgs, string outputDir)
        {
            _sdb = sdb;
            _smoScriptOpts = options;
            _updateStatus = updateStatus;
            _eventArgs = eventArgs;
            _outputDir = outputDir;
        }

        public bool Process(SqlSmoObject[] objects, int start)
        {
            if (objects == null) return AsyncProcessingStatus.CancelProcessing;

            SqlSmoObject[] sso = new SqlSmoObject[1];
            StringCollection tableForeignKey = new StringCollection();
            StringCollection extendedProperties = new StringCollection();
            StringCollection bcpOutputCommands = new StringCollection();
            StringCollection bcpTargetCommands = new StringCollection();
            double step = ((100 - start) / (double)objects.Count());
            int loopCnt = 0;

            bool oldSQLdb = _smoScriptOpts.TargetServer.Equals(Properties.Resources.ServerType_AzureSQLDatabase);

            List<string> listAssemblyUDF = new List<string>();
            List<string> listEncryptedUDF = new List<string>();

            List<string> listEncryptedView = new List<string>();

            List<string> listAssemblySP = new List<string>();
            List<string> listEncryptedSP = new List<string>();

            List<string> listAssemblyDDLT = new List<string>();
            List<string> listEncryptedDDLT = new List<string>();


            foreach (SqlSmoObject obj in objects)
            {
                loopCnt++;

                _eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.ScriptingObj, obj.ToString());
                _eventArgs.PercentComplete = (int)(loopCnt * step);
                _updateStatus(_eventArgs);

                sso[0] = obj;
                switch (obj.Urn.Type)
                {
                    case "Table":
                        _sdb.ScriptTables(sso, ref tableForeignKey, ref extendedProperties, ref bcpOutputCommands, ref bcpTargetCommands, ref _outputDir);
                        break;

                    case "UserDefinedFunction":
                        UserDefinedFunction udf = (UserDefinedFunction)obj;
                        if (udf.IsEncrypted)
                        {
                            if (udf.AssemblyName.Length > 0)
                            {
                                if (oldSQLdb)
                                {
                                    listAssemblyUDF.Add(udf.TextBody);
                                }
                                else
                                {
                                    _sdb.ScriptUDF(sso);
                                }
                            }
                            else
                            {
                                listEncryptedUDF.Add(udf.ToString());
                            }
                        }
                        else
                        {
                            _sdb.ScriptUDF(sso);
                        }
                        break;

                    case "View":
                        View vw = (View)obj;
                        if (vw.IsEncrypted)
                        {
                            listEncryptedView.Add(vw.ToString());
                        }
                        else
                        {
                            _sdb.ScriptViews(sso);
                        }
                        break;

                    case "StoredProcedure":
                        StoredProcedure proc = (StoredProcedure)obj;
                        if (proc.IsEncrypted)
                        {
                            if (proc.AssemblyName.Length > 0)
                            {
                                if (oldSQLdb)
                                {
                                    listAssemblySP.Add(proc.TextBody);
                                }
                                else
                                {
                                    _sdb.ScriptProcedures(sso);
                                }
                            }
                            else
                            {
                                listEncryptedSP.Add(proc.ToString());
                            }
                        }
                        else
                        {
                            _sdb.ScriptProcedures(sso);
                        }
                        break;

                    case "DdlTrigger":
                        DatabaseDdlTrigger ddlt = (DatabaseDdlTrigger)obj;
                        if (ddlt.IsEncrypted)
                        {
                            if (ddlt.AssemblyName.Length > 0)
                            {
                                if (oldSQLdb)
                                {
                                    listAssemblyDDLT.Add(ddlt.TextBody);
                                }
                                else
                                {
                                    _sdb.ScriptTriggers(sso);
                                }
                            }
                            else
                            {
                                listEncryptedDDLT.Add(ddlt.ToString());
                            }
                        }
                        else
                        {
                            _sdb.ScriptTriggers(sso);
                        }
                        break;
                }

                if (AsyncProcessingStatus.CancelProcessing)
                {
                    _eventArgs.StatusMsg = Properties.Resources.MessageCanceled;
                    _eventArgs.DisplayColor = Color.DarkCyan;
                    _eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageCanceledProcessing, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
                    _eventArgs.PercentComplete = 100;
                    _updateStatus(_eventArgs);
                    return AsyncProcessingStatus.CancelProcessing;
                }
            }

            OutputNotSupported(Properties.Resources.ObjectTypeUDF, listAssemblyUDF);
            OutputIsEncrypted(Properties.Resources.ObjectTypeUDF, listEncryptedUDF);

            OutputIsEncrypted(Properties.Resources.ObjectTypeViews, listEncryptedView);

            OutputNotSupported(Properties.Resources.ObjectTypeStoredProcedures, listAssemblySP);
            OutputIsEncrypted(Properties.Resources.ObjectTypeStoredProcedures, listEncryptedSP);

            OutputNotSupported(Properties.Resources.ObjectTypeTriggers, listAssemblyDDLT);
            OutputIsEncrypted(Properties.Resources.ObjectTypeTriggers, listEncryptedDDLT);

            _eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.ForiegnKeys);
            _updateStatus(_eventArgs);

            // Ok, due to dependency issues, we need to move the foreign key constraints to the end.
            // But there is some small problem in that SQL Server allows for a DBA to have circular dependencies.  So
            // in order to solve this problem, we need to upload the data to the circular dependent tables first
            // with no foreign keys, add foreign key constraints, and then upload the rest of the data.  The
            // reason that the data it not all uploaded before the foreign key constraints is to avoid performance
            // issues with adding foreign key constraints after the fact.

            if (_smoScriptOpts.ScriptForeignKeys == true)
            {
                _sdb.ScriptStringCollection(ref tableForeignKey, Color.Black);
            }

            if (_smoScriptOpts.ScriptExtendedProperties == true)
            {
                _sdb.ScriptStringCollection(ref extendedProperties, Color.Black);
            }

            if (bcpTargetCommands.Count > 0)
            {
                _sdb.ScriptStringCollection(ref bcpTargetCommands, Color.Green);
            }

            if (bcpOutputCommands.Count > 0)
            {
                _sdb.OutputAnalysisLine(Environment.NewLine + Properties.Resources.BCPOutputSummary, Color.Green);
                foreach (string bcpOutputCommand in bcpOutputCommands)
                {
                    _sdb.OutputAnalysisLine(bcpOutputCommand, Color.Green);
                }
            }
            return AsyncProcessingStatus.CancelProcessing;
        }

        private void OutputNotSupported(string type, List<string> list)
        {
            if (list.Count < 1) return;

            _sdb.OutputAnalysisLine(string.Format(Properties.Resources.NotSupported, type), Color.Red);
            foreach (string name in list)
            {
                _sdb.OutputAnalysisLine("    " + name, Color.Red);
            }
            _sdb.OutputAnalysisLine("", Color.Red);
        }

        private void OutputIsEncrypted(string type, List<string> list)
        {
            if (list.Count < 1) return;

            _sdb.OutputAnalysisLine(string.Format(Properties.Resources.IsEncrypted, type), Color.Red);
            foreach (string name in list)
            {
                _sdb.OutputAnalysisLine("    " + name, Color.Red);
            }
            _sdb.OutputAnalysisLine("", Color.Red);
        }

        public bool Process(DatabaseObjectsTypes objectType, SqlSmoObject[] objectList, int percent)
        {
            if (objectList != null && objectList.Count<SqlSmoObject>() > 0)
            {
                _eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.ScriptingObj, objectType);
                _eventArgs.PercentComplete = percent;
                _updateStatus(_eventArgs);

                List<Trigger> listSupported = new List<Trigger>();
                List<string> listNotSupported = new List<string>();
                List<string> listEncrypted = new List<string>();

                switch (objectType)
                {
                    case DatabaseObjectsTypes.Assemblies:
                        if (_smoScriptOpts.TargetServer == Properties.Resources.ServerType_AzureSQLDatabase)
                        {
                            foreach (SqlAssembly asm in objectList)
                            {
                                listNotSupported.Add("    " + asm.ToString());
                            }
                            OutputNotSupported(Properties.Resources.ObjectTypeSQLAssemblies, listNotSupported);
                            _sdb.OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentStart), Color.DarkGreen);
                            _sdb.OutputSQLString(Properties.Resources.RemoveComment + " " + string.Format(Properties.Resources.NotSupported, Properties.Resources.ObjectTypeSQLAssemblies), Color.Red);
                            _sdb.ScriptAssemblies(objectList);
                            _sdb.OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentEnd), Color.DarkGreen);
                        }
                        else
                        {
                            _sdb.ScriptAssemblies(objectList);
                        }
                        break;

                    case DatabaseObjectsTypes.PartitionFunctions:
                        if (_smoScriptOpts.TargetServer == Properties.Resources.ServerType_AzureSQLDatabase)
                        {
                            foreach (PartitionFunction pf in objectList)
                            {
                                listNotSupported.Add("    " + pf.ToString());
                            }
                            OutputNotSupported(Properties.Resources.ObjectTypePartitionFunctions, listNotSupported);
                            _sdb.OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentStart), Color.DarkGreen);
                            _sdb.OutputSQLString(Properties.Resources.RemoveComment + " " + string.Format(Properties.Resources.NotSupported, Properties.Resources.ObjectTypePartitionFunctions), Color.Red);
                            _sdb.ScriptPartitionFunctions(objectList);
                            _sdb.OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentEnd), Color.DarkGreen);
                        }
                        else
                        {
                            _sdb.ScriptPartitionFunctions(objectList);
                        }
                        break;

                    case DatabaseObjectsTypes.PartitionSchemes:
                        if (_smoScriptOpts.TargetServer == Properties.Resources.ServerType_AzureSQLDatabase)
                        {
                            foreach (PartitionScheme ps in objectList)
                            {
                                listNotSupported.Add("    " + ps.ToString());
                            }
                            OutputNotSupported(Properties.Resources.ObjectTypePartitionSchemes, listNotSupported);
                            _sdb.OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentStart), Color.DarkGreen);
                            _sdb.OutputSQLString(Properties.Resources.RemoveComment + " " + string.Format(Properties.Resources.NotSupported, Properties.Resources.ObjectTypePartitionSchemes), Color.Red);
                            _sdb.ScriptPartitionSchemes(objectList);
                            _sdb.OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentEnd), Color.DarkGreen);
                        }
                        else
                        {
                            _sdb.ScriptPartitionSchemes(objectList);
                        }
                        break;

                    case DatabaseObjectsTypes.Roles:
                        _sdb.ScriptRoles(objectList);
                        break;

                    case DatabaseObjectsTypes.Schemas:
                        _sdb.ScriptSchemas(objectList);
                        break;

                    case DatabaseObjectsTypes.Synonyms:
                        _sdb.ScriptSynonyms(objectList);
                        break;

                    case DatabaseObjectsTypes.Triggers:
                        foreach (Trigger tig in objectList)
                        {
                            if (tig.IsEncrypted)
                            {
                                if (tig.AssemblyName.Length > 0)
                                {
                                    if (_smoScriptOpts.TargetServer == Properties.Resources.ServerType_AzureSQLDatabase)
                                    {
                                        listNotSupported.Add("    " + tig.ToString());
                                        break;
                                    }
                                }
                                else
                                {
                                    listEncrypted.Add("     " + tig.ToString());
                                    break;
                                }
                            }
                            listSupported.Add(tig);
                        }
                        OutputNotSupported(Properties.Resources.ObjectTypeTriggers, listNotSupported);
                        OutputIsEncrypted(Properties.Resources.ObjectTypeTriggers, listEncrypted);
                        if (listSupported.Count > 0) _sdb.ScriptTriggers(listSupported.ToArray());
                        break;

                    case DatabaseObjectsTypes.UserDefinedDataTypes:
                        _sdb.ScriptUDT(objectList);
                        break;

                    case DatabaseObjectsTypes.UserDefinedTableTypes:
                        _sdb.ScriptUDTT(objectList);
                        break;

                    case DatabaseObjectsTypes.XMLSchemaCollections:
                        _sdb.ScriptXMLSchemaCollections(objectList);
                        break;
                }
            }

            if (AsyncProcessingStatus.CancelProcessing)
            {
                _eventArgs.DisplayColor = Color.DarkCyan;
                _eventArgs.StatusMsg = Properties.Resources.MessageCanceled;
                _eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageCanceledProcessing, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
                _eventArgs.PercentComplete = 100;
                _updateStatus(_eventArgs);
            }
            return AsyncProcessingStatus.CancelProcessing;
        }
    }
}