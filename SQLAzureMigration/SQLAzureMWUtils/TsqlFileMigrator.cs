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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace SQLAzureMWUtils
{
    public class TsqlFileMigrator
    {
        private IMigrationOutput _Output = null;
        public string _FileToProcess = null;
        public string _TargetFile = null;
        private StreamWriter _SwTsql = null;
        public int _FileIndex;

        public TsqlFileMigrator(string fileToProcess, int index, string targetFile, IMigrationOutput output)
        {
            this._Output = output;
            this._FileToProcess = fileToProcess;
            this._TargetFile = targetFile;
            this._FileIndex = index;
        }

        public TsqlFileMigrator(string fileToProcess, IMigrationOutput output)
        {
            this._Output = output;
            this._FileToProcess = fileToProcess;
        }

        public void ParseFile(SMOScriptOptions options, bool _ParseFile)
        {
            //create a dummy asyncNotificationEventArg obj for cmdline scenario. TODO: ensure this works for console commandline
            AsyncNotificationEventArgs e = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ParseFolder, 0, "", "", Color.Black);
            ParseFile(options, _ParseFile, e);
        }

        public bool ParseFile(SMOScriptOptions options, bool _ParseFile, AsyncNotificationEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            ScriptDatabase sdb = new ScriptDatabase();

            if (e.FunctionCode==NotificationEventFunctionCode.ParseFolder)
            {
                FileInfo fi = new FileInfo(_FileToProcess);
                _SwTsql = new StreamWriter(_TargetFile, false);
                sdb.Initialize(_Output.StatusUpdateHandler, options, false, _FileToProcess, _SwTsql);
            }
            else 
            {
                sdb.Initialize(_Output.StatusUpdateHandler, options, false);
            }
            /****************************************************************/
            string sqlText = CommonFunc.GetTextFromFile(_FileToProcess);
            CommentAreaHelper cah = new CommentAreaHelper();
            long totalCharacterOffset = 0;
            bool bCommentedLine = false;

            List<string> sqlCmds = new List<string>();
            if (_ParseFile)
            {
                StringBuilder sb = new StringBuilder();
                cah.FindCommentAreas(sqlText);
                foreach (string line in cah.Lines)
                {
                    if (line.Equals(Properties.Resources.Go, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!cah.IsIndexInComments(totalCharacterOffset))
                        {
                            sqlCmds.Add(sb.ToString());
                            sb.Length = 0;
                        }
                        else
                        {
                            sb.Append(line + Environment.NewLine);
                        }
                    }
                    else
                    {
                        sb.Append(line + Environment.NewLine);
                    }
                    totalCharacterOffset += line.Length + cah.CrLf;
                }

                if (sb.Length > 0)
                {
                    sqlCmds.Add(sb.ToString());
                    sb.Length = 0;
                }
            }
            else
            {
                sqlCmds.Add(sqlText);
            }

            int numCmds = sqlCmds.Count();
            int loopCtr = 0;
            if (numCmds == 0)
            {
                e.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageNoDataToProcess + Environment.NewLine, _FileToProcess);
                if (e.FunctionCode == NotificationEventFunctionCode.ParseFile)
                {
                    e.PercentComplete = 100;
                }
                else if(e.FunctionCode == NotificationEventFunctionCode.ParseFolder)
                {
                    e.FilesProcessed++;
                }
                _Output.StatusUpdateHandler(e);
                return false;
            }
            e.DisplayColor = Color.DarkBlue;
            e.DisplayText = "";
            if (e.FunctionCode == NotificationEventFunctionCode.ParseFile)
            {
                e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingStatus, loopCtr.ToString(), numCmds.ToString()) ;
                e.PercentComplete = 0;
            }
            else if (e.FunctionCode == NotificationEventFunctionCode.ParseFolder)
            {
                e.TotalDenominator = e.TotalDenominator + numCmds;
            }
            _Output.StatusUpdateHandler(e);
            totalCharacterOffset = 0;

            foreach (string cmd in sqlCmds)
            {
                ++loopCtr;

                if (AsyncProcessingStatus.CancelProcessing) break;
                if (cmd.Length == 0 || cmd.Equals(Environment.NewLine)) continue;

                foreach (CommentArea ca in cah.CommentAreas)
                {
                    if (ca.Start == totalCharacterOffset && ca.End == totalCharacterOffset + cmd.Length - cah.CrLf - 1) // note that the -1 is to put you at zero based counting
                    {
                        bCommentedLine = true;
                        break;
                    }
                    bCommentedLine = false;
                }
                totalCharacterOffset += cmd.Length + cah.CrLf;

                if (_ParseFile && !bCommentedLine && !(cmd.StartsWith("/*~") || cmd.StartsWith("~*/")))
                {
                    if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sPROCEDURE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileTSQLGo(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sTABLE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileTable(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "CREATE\\sXML\\sSCHEMA\\sCOLLECTION", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileXMLSchemaCollections(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "CREATE\\sTYPE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileUDT(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "CREATE\\s[a-z\\s]*\\sINDEX", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileIndex(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sROLE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileRole(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sSYNONYM", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileSynonyms(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sSCHEMA", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileSchemas(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sASSEMBLY", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileAssemblies(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sPARTITIONFUNCTIONS", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFilePartitionFunctions(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sPARTITIONSCHEMES", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFilePartitionSchemes(cmd);
                    }
                    else
                    {
                        sdb.ParseFileTSQLGo(cmd);
                    }
                    if (e.FunctionCode == NotificationEventFunctionCode.ParseFolder)
                    {
                        e.NumeratorComplete = e.NumeratorComplete + 1;
                    }
                }
                else
                {
                    sdb.OutputSQLString(cmd, Color.Black);
                }
                if (loopCtr % 20 == 0)
                {
                    e.DisplayColor = Color.DarkBlue;
                    e.DisplayText = "";
                    if (e.FunctionCode == NotificationEventFunctionCode.ParseFile)
                    {
                        e.PercentComplete = (int)(((float)loopCtr / (float)numCmds) * 100.0);
                        e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingStatus, loopCtr.ToString(), numCmds.ToString());
                    }
                    else if (e.FunctionCode == NotificationEventFunctionCode.ParseFolder)
                    {
                        e.PercentComplete = (int)(((float)e.NumeratorComplete / (float)e.TotalDenominator) * 100);
                    }
                     _Output.StatusUpdateHandler(e);
                }
            }
            DateTime endTime = DateTime.Now;
            if (e.FunctionCode == NotificationEventFunctionCode.ParseFile)
            {
                if (AsyncProcessingStatus.CancelProcessing)
                {
                    e.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageCanceledProcessing, DateTime.Now.ToString()) + Environment.NewLine;
                    e.StatusMsg = Properties.Resources.MessageCanceled;
                }
                else
                {
                        e.DisplayColor = Color.DarkCyan;
                        if (_ParseFile)
                        {
                            e.DisplayText = Properties.Resources.AnalysisComplete + Environment.NewLine;
                        }
                        else
                        {
                            e.DisplayText = Properties.Resources.MessageFileReadAndReady;
                        }
                        e.StatusMsg = Properties.Resources.Done;
                }
                e.PercentComplete = 100;
                e.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageTotalFileProcessingTime + Environment.NewLine,
                endTime.Subtract(startTime).ToString(),
                Properties.Resources.RemoveComment,
                _FileToProcess.ToString());
                _Output.StatusUpdateHandler(e);
            }   
            else if (e.FunctionCode == NotificationEventFunctionCode.ParseFolder)
            {
                _SwTsql.Flush();
                _SwTsql.Close();
                if (sdb.IssuesFound == true)
                {
                    e.DisplayText = Properties.Resources.MessageFileChangedState +
                        CommonFunc.FormatString(Properties.Resources.MessageTotalFileProcessingTime,
                        endTime.Subtract(startTime).ToString(), _FileToProcess.ToString()) + Environment.NewLine;
                    e.DisplayColor = Color.Brown;
                }
                else
                {
                    
                    e.DisplayText = Properties.Resources.MessageFileNoChangeState +
                        CommonFunc.FormatString(Properties.Resources.MessageTotalFileProcessingTime,
                        endTime.Subtract(startTime).ToString(), _FileToProcess.ToString()) + Environment.NewLine;
                    e.DisplayColor = Color.DarkBlue;
                }
                _Output.StatusUpdateHandler(e);
                e.FilesProcessed++;
            }
            return sdb.IssuesFound;
        }
    }
}