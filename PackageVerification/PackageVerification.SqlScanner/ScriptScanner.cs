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
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PackageVerification.SqlScanner.Models;
using SQLAzureMWUtils;

namespace PackageVerification.SqlScanner
{
    public class ScriptScanner : SqlScannerBaseClass
    {
        private IMigrationOutput _output;

        public List<Azure> ProcessAzure(string inputFilePath, string outputFilePath)
        {
            try
            {
                using (var writer = new StreamWriter(File.Create(outputFilePath)))
                {
                    writer.WriteLine();
                    writer.WriteLine("-- SQLAzureMWParseTSQL TSQL Output File:");
                    writer.WriteLine("-- Generated at: {0}", DateTime.Now);
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sorry, error opening output file {0}.  Error: {1}", outputFilePath, ex.Message);
                return null;
            }

            _output = new ConsoleMigrationOutput(outputFilePath, true);
            
            return ParseFile(inputFilePath, true);
        }

        private List<Azure> ParseFile(string fileToProcess, bool parseFile)
        {
            var startTime = DateTime.Now;
            var messageList = new List<Azure> {new Azure(fileToProcess, MessageTypes.Info, "Beginning Azure Check.")};

            var sdb = new ScriptDatabase();
            var options = SMOScriptOptions.CreateFromConfig();
            sdb.Initialize(_output.StatusUpdateHandler, options, false);

            /****************************************************************/

            var sqlText = CommonFunc.GetTextFromFile(fileToProcess);
            var cah = new CommentAreaHelper();
            long totalCharacterOffset = 0;
            var bCommentedLine = false;

            var sqlCmds = new List<string>();
            if (parseFile)
            {
                var sb = new StringBuilder();
                cah.FindCommentAreas(sqlText);
                foreach (var line in cah.Lines)
                {
                    if (line.Equals("GO", StringComparison.OrdinalIgnoreCase))
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

                //first we will remove the comments.
                var r = new Regex(@"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(--.*)");
                var remainder = r.Replace(sb.ToString(), string.Empty).Trim();

                if (remainder.Length > 8)
                {
                    messageList.Add(new Azure(fileToProcess, MessageTypes.Warning, "Final Section of T-SQL didn't finish with a 'GO' command."));
                    sb.Append("GO" + Environment.NewLine);
                    sqlCmds.Add(sb.ToString());
                }
                else //this will re-add the comments and such so that the output files will better match the input files.
                {
                    sqlCmds.Add(sb.ToString());
                }
            }
            else
            {
                sqlCmds.Add(sqlText);
            }

            var numCmds = sqlCmds.Count;

            if (numCmds == 0)
            {
                messageList.Add(new Azure(fileToProcess, MessageTypes.Info, "No data to process"));

                return messageList;
            }

            var loopCtr = 0;

            messageList.Add(new Azure(fileToProcess, MessageTypes.Info, string.Format("Processing {0} out of {1}", loopCtr, numCmds)));

            totalCharacterOffset = 0;

            foreach (var cmd in sqlCmds)
            {
                ++loopCtr;

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

                if (parseFile && !bCommentedLine && !(cmd.StartsWith("/*~") || cmd.StartsWith("~*/")))
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
                    else if (Regex.IsMatch(cmd, "CREATE ROLE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileRole(cmd);
                    }
                    else
                    {
                        sdb.ParseFileTSQLGo(cmd);
                    }
                }
                else
                {
                    sdb.OutputSQLString(cmd, Color.Black);
                }

                

                if (loopCtr % 20 == 0)
                {
                    messageList.Add(new Azure(fileToProcess, MessageTypes.Info, string.Format("Processing {0} out of {1} - %{2} Complete", loopCtr, numCmds, (int)((loopCtr / (float)numCmds) * 100.0))));
                }
            }

            var endTime = DateTime.Now;

            messageList.Add(new Azure(fileToProcess, MessageTypes.Info, string.Format(
                "{1}Total processing time --> {0}",
                endTime.Subtract(startTime),
                "--~")));

            return messageList;
        }

        public List<Azure> LogAzure(string fileToProcess)
        {
            var messageList = new List<Azure> ();
            var sqlText = CommonFunc.GetTextFromFile(fileToProcess);
            var cah = new CommentAreaHelper();
            
            cah.FindCommentAreas(sqlText);
            foreach (var line in cah.Lines)
            {
                if(line.StartsWith("--~"))
                {
                    var azureMessage = line.Substring(4, line.Length - 4);
                    messageList.Add(new Azure(fileToProcess, MessageTypes.Error, azureMessage));
                }
            }

            return messageList;
        }
    }
}