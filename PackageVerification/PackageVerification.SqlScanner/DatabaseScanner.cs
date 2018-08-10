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
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PackageVerification.SQLRunner.Models;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SQLAzureMWUtils;
using Database = Microsoft.SqlServer.Management.Smo.Database;

namespace PackageVerification.SqlScanner
{
    public class DatabaseScanner : SqlScannerBaseClass
    {
        List<string> output = new List<string>(); 

        public List<string> GenerateScriptFromSourceServer(ServerConnection sc, List<DatabaseObject> objectList)
        {
            var dtStart = DateTime.Now;
            AsyncUpdateStatus updateStatus = GenScriptAsyncUpdateStatusHandler;
            var args = new AsyncNotificationEventArgs(NotificationEventFunctionCode.GenerateScriptFromSQLServer, 0, "", CommonFunc.FormatString("Process started at {0} -- UTC -> {1} ... ", dtStart.ToString(CultureInfo.CurrentUICulture), dtStart.ToUniversalTime().ToString(CultureInfo.CurrentUICulture)) + Environment.NewLine, Color.DarkBlue);
            StreamWriter swTSQL = null;
            SqlSmoObject[] smoTriggers = null;
            Object sender = System.Threading.Thread.CurrentThread;
            var smoScriptOpts = SMOScriptOptions.CreateFromConfig();
            updateStatus(args);
            
            sc.Connect();

            var ss = new Server(sc);
            var db = ss.Databases[sc.DatabaseName];
            var sdb = new ScriptDatabase();
            sdb.Initialize(ss, db, updateStatus, smoScriptOpts, swTSQL);

            args.DisplayText = "";
            args.StatusMsg = "Sorting objects by dependency ...";
            args.PercentComplete = 1;
            updateStatus(args);

            var sorted = GetSortedObjects(db, objectList);
            var sp = new SourceProcessor();
            var tempPathForBCPData = Path.Combine(Path.GetTempPath(), "/BCPData");
            Directory.CreateDirectory(tempPathForBCPData);
            sp.Initialize(sdb, smoScriptOpts, updateStatus, args, tempPathForBCPData);

            //NOTE: This is what does the magic!
            if (sp.Process(sorted, 30)) return output;

            if (!Regex.IsMatch(smoScriptOpts.ScriptTableAndOrData, smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase))
            {
                if (sp.Process(DatabaseObjectsTypes.Triggers, smoTriggers, 95)) return output;
            }

            var dtEnd = DateTime.Now;
            var tsDuration = dtEnd.Subtract(dtStart);
            var sHour = tsDuration.Minutes == 1 ? " hour, " : " hours, ";
            var sMin = tsDuration.Minutes == 1 ? " minute and " : " minutes and ";
            var sSecs = tsDuration.Seconds == 1 ? " second" : " seconds";

            args.StatusMsg = "Done!";
            args.DisplayColor = Color.DarkCyan;

            if (smoScriptOpts.CheckCompatibility() == 1)
            {
                args.DisplayText = CommonFunc.FormatString(@"No analysis done on script.
Processing finished at {0} -- UTC -> {1}
Total processing time: {2}", dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), tsDuration.Hours + sHour + tsDuration.Minutes.ToString(CultureInfo.CurrentUICulture) + sMin + tsDuration.Seconds.ToString(CultureInfo.CurrentUICulture) + sSecs);
            }
            else
            {
                args.DisplayText = CommonFunc.FormatString(@"Analysis completed at {0} -- UTC -> {1}
Any issues discovered will be reported above.
Total processing time: {2}", dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), tsDuration.Hours + sHour + tsDuration.Minutes.ToString(CultureInfo.CurrentUICulture) + sMin + tsDuration.Seconds.ToString(CultureInfo.CurrentUICulture) + sSecs);
            }
            args.PercentComplete = 100;
            updateStatus(args);
            return output;
        }

        public SMOScriptOptions CreateFromConfig()
        {
            var smoScriptOpts = new SMOScriptOptions();
            var target = CommonFunc.GetAppSettingsStringValue("TargetServerType");

            if (!string.IsNullOrEmpty(target))
            {
                smoScriptOpts.TargetServer = target;
            }

            smoScriptOpts.ScriptDefaults = CommonFunc.GetAppSettingsBoolValue("ScriptDefaults");
            smoScriptOpts.ScriptHeaders = CommonFunc.GetAppSettingsBoolValue("ScriptHeaders");
            smoScriptOpts.IncludeIfNotExists = CommonFunc.GetAppSettingsBoolValue("IncludeIfNotExists");
            smoScriptOpts.ScriptDropCreate = CommonFunc.GetAppSettingsStringValue("ScriptDropCreate");
            smoScriptOpts.ScriptCheckConstraints = CommonFunc.GetAppSettingsBoolValue("ScriptCheckConstraints");
            smoScriptOpts.ScriptCollation = CommonFunc.GetAppSettingsBoolValue("ScriptCollation");
            smoScriptOpts.ScriptForeignKeys = CommonFunc.GetAppSettingsBoolValue("ScriptForeignKeys");
            smoScriptOpts.ScriptPrimaryKeys = CommonFunc.GetAppSettingsBoolValue("ScriptPrimaryKeys");
            smoScriptOpts.ScriptUniqueKeys = CommonFunc.GetAppSettingsBoolValue("ScriptUniqueKeys");
            smoScriptOpts.ScriptIndexes = CommonFunc.GetAppSettingsBoolValue("ScriptIndexes");
            smoScriptOpts.ScriptTableAndOrData = CommonFunc.GetAppSettingsStringValue("ScriptTableAndOrData");

            if (smoScriptOpts.ScriptTableAndOrData.Length == 0)
            {
                smoScriptOpts.ScriptTableAndOrData = CommonFunc.GetAppSettingsBoolValue("ScriptData") ? "Table Schema with Data" : "Table Schema Only";
            }

            smoScriptOpts.ScriptTableTriggers = CommonFunc.GetAppSettingsBoolValue("ScriptTableTriggers");
            smoScriptOpts.ActiveDirectorySP = CommonFunc.GetAppSettingsBoolValue("ActiveDirectorySP");
            smoScriptOpts.BackupandRestoreTable = CommonFunc.GetAppSettingsBoolValue("BackupandRestoreTable");
            smoScriptOpts.ChangeDataCapture = CommonFunc.GetAppSettingsBoolValue("ChangeDataCapture");
            smoScriptOpts.ChangeDataCaptureTable = CommonFunc.GetAppSettingsBoolValue("ChangeDataCaptureTable");
            smoScriptOpts.CompatibilityChecks = CommonFunc.GetAppSettingsStringValue("CompatibilityChecks");
            smoScriptOpts.DatabaseEngineSP = CommonFunc.GetAppSettingsBoolValue("DatabaseEngineSP");
            smoScriptOpts.DatabaseMailSP = CommonFunc.GetAppSettingsBoolValue("DatabaseMailSP");
            smoScriptOpts.DatabaseMaintenancePlan = CommonFunc.GetAppSettingsBoolValue("DatabaseMaintenancePlan");
            smoScriptOpts.DataControl = CommonFunc.GetAppSettingsBoolValue("DataControl");
            smoScriptOpts.DistributedQueriesSP = CommonFunc.GetAppSettingsBoolValue("DistributedQueriesSP");
            smoScriptOpts.FullTextSearchSP = CommonFunc.GetAppSettingsBoolValue("FullTextSearchSP");
            smoScriptOpts.GeneralExtendedSPs = CommonFunc.GetAppSettingsBoolValue("GeneralExtendedSPs");
            smoScriptOpts.GeneralTSQL = CommonFunc.GetAppSettingsBoolValue("GeneralTSQL");
            smoScriptOpts.IntegrationServicesTable = CommonFunc.GetAppSettingsBoolValue("IntegrationServicesTable");
            smoScriptOpts.LogShipping = CommonFunc.GetAppSettingsBoolValue("LogShipping");
            smoScriptOpts.MetadataFunction = CommonFunc.GetAppSettingsBoolValue("MetadataFunction");
            smoScriptOpts.OLEAutomationSP = CommonFunc.GetAppSettingsBoolValue("OLEAutomationSP");
            smoScriptOpts.OLEDBTable = CommonFunc.GetAppSettingsBoolValue("OLEDBTable");
            smoScriptOpts.ProfilerSP = CommonFunc.GetAppSettingsBoolValue("ProfilerSP");
            smoScriptOpts.ReplicationSP = CommonFunc.GetAppSettingsBoolValue("ReplicationSP");
            smoScriptOpts.ReplicationTable = CommonFunc.GetAppSettingsBoolValue("ReplicationTable");
            smoScriptOpts.RowsetFunction = CommonFunc.GetAppSettingsBoolValue("RowsetFunction");
            smoScriptOpts.SecurityFunction = CommonFunc.GetAppSettingsBoolValue("SecurityFunction");
            smoScriptOpts.SecuritySP = CommonFunc.GetAppSettingsBoolValue("SecuritySP");
            smoScriptOpts.SQLMailSP = CommonFunc.GetAppSettingsBoolValue("SQLMailSP");
            smoScriptOpts.SQLServerAgentSP = CommonFunc.GetAppSettingsBoolValue("SQLServerAgentSP");
            smoScriptOpts.SQLServerAgentTable = CommonFunc.GetAppSettingsBoolValue("SQLServerAgentTable");
            smoScriptOpts.SystemCatalogView = CommonFunc.GetAppSettingsBoolValue("SystemCatalogView");
            smoScriptOpts.SystemFunction = CommonFunc.GetAppSettingsBoolValue("SystemFunction");
            smoScriptOpts.SystemStatisticalFunction = CommonFunc.GetAppSettingsBoolValue("SystemStatisticalFunction");
            smoScriptOpts.Unclassified = CommonFunc.GetAppSettingsBoolValue("Unclassified");

            return smoScriptOpts;
        }

        private void GenScriptAsyncUpdateStatusHandler(AsyncNotificationEventArgs args)
        {
            if (args.FunctionCode == NotificationEventFunctionCode.AnalysisOutput)
            {
                if (args.DisplayColor == Color.Red || args.DisplayColor == Color.Brown)
                {
                    //Console.WriteLine(args.DisplayText);
                    output.Add(args.DisplayText);
                }
            }
        }

        private SqlSmoObject[] GetSortedObjects(Database sourceDatabase, List<DatabaseObject> objectList)
        {
            // Ok, we need to get all of the selected objects and put them into one array
            // so that we can get them sorted by dependency.
            
            var output = new List<SqlSmoObject>();

            foreach (var databaseObject in objectList)
            {
                switch (databaseObject.Type.Trim())
                {
                    //U = Table
                    case "U":
                        output.AddRange(sourceDatabase.Tables.Cast<SqlSmoObject>().Where(obj => obj.ToString().Equals(databaseObject.FullSafeName)));
                        break;
                    //P = SQL stored procedure
                    case "P":
                        output.AddRange(sourceDatabase.StoredProcedures.Cast<SqlSmoObject>().Where(obj => obj.ToString().Equals(databaseObject.FullSafeName)));
                        break;
                    //V 	View
                    case "V":
                        output.AddRange(sourceDatabase.Views.Cast<SqlSmoObject>().Where(obj => obj.ToString().Equals(databaseObject.FullSafeName)));
                        break;
                    //FN 	SQL scalar function
                    case "FN":
                    //IF 	SQL inline table-valued function
                    case "IF":
                    //TF 	SQL table-valued-function 
                    case "TF":
                        output.AddRange(sourceDatabase.UserDefinedFunctions.Cast<SqlSmoObject>().Where(obj => obj.ToString().Equals(databaseObject.FullSafeName)));
                        break;
                    default:
                        break;
                }
            }
            
            return Sort(sourceDatabase, output.ToArray());
        }

        private SqlSmoObject[] Sort(Database sourceDatabase, SqlSmoObject[] smoObjects)
        {
            if (smoObjects.Count() < 2) return smoObjects;

            DependencyTree dt = null;
            var dw = new DependencyWalker(sourceDatabase.Parent);

            try
            {
                dt = dw.DiscoverDependencies(smoObjects, true);
            }
            catch //(Exception ex)
            {
                //ErrorHelper.ShowException(this, ex);
                return smoObjects;
            }

            var sorted = new SqlSmoObject[smoObjects.Count()];
            var index = 0;

            foreach (var d in dw.WalkDependencies(dt))
            {
                if (d.Urn.Type.Equals("UnresolvedEntity")) continue;

                foreach (var sso in smoObjects)
                {
                    if (sso.Urn.ToString().Equals(d.Urn, StringComparison.CurrentCultureIgnoreCase))
                    {
                        sorted[index++] = sso;
                        break;
                    }
                }
            }
            return sorted;
        }
    }
}