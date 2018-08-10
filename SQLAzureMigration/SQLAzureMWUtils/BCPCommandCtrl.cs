﻿#region Copyright
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Globalization;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace SQLAzureMWUtils
{
    public partial class BCPCommandCtrl : UserControl
    {
        public AsyncBCPJobUpdateStatus UpdateBCPJobStatus { get; set; }
        private BCPJobInfo[] _CurrentJobInfo = new BCPJobInfo[1];
        private StringBuilder _bcpOutput = new StringBuilder();
        private object _stringBuilderLock = new object();

        public RichTextBox Results
        {
            get
            {
                return rtbResults;
            }
        }

        public BCPJobInfo CurrentJobInfo
        {
            get
            {
                return _CurrentJobInfo[0];
            }

            set
            {
                _CurrentJobInfo[0] = value;
            }
        }

        public BCPCommandCtrl()
        {
            InitializeComponent();
        }

        private void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs args)
        {
            if (!String.IsNullOrEmpty(args.Data))
            {
                lock (_stringBuilderLock)
                {
                    Debug.WriteLine(args.Data);
                    _bcpOutput.Append(args.Data + Environment.NewLine);
                }
            }
        }

        public void NotifyJobIsFinished()
        {
            ++AsyncProcessingStatus.NumberOfCommandsExecuted;
            AsyncNotificationEventArgs eventArgs = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ExecuteSqlOnAzure, 0, "", "", Color.DarkSlateBlue);
            eventArgs.PercentComplete = (int)(((float)AsyncProcessingStatus.NumberOfCommandsExecuted / (float)AsyncProcessingStatus.NumberOfCommands) * 100.0);
            eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.BCPProcessingStatus, AsyncProcessingStatus.NumberOfCommandsExecuted.ToString(), AsyncProcessingStatus.NumberOfCommands.ToString());
            CurrentJobInfo.UpdateStatus(eventArgs);
        }

        private string GetThreadSafeBCPOutputString()
        {
            lock (_stringBuilderLock)
            {
                return _bcpOutput.ToString();
            }
        }
        
        public void ExecuteBCPCommand()
        {
            string bcpOutputFile = "";
            string bcpOutputFileBase = "";
            long rowsCopied = 0;
            long totalUploaded = 0;
            long chunkSize = Convert.ToInt32(ConfigurationManager.AppSettings["ChunkSize"], CultureInfo.InvariantCulture);
            int sleep = Convert.ToInt32(ConfigurationManager.AppSettings["TimeBetweenChunks"], CultureInfo.InvariantCulture);
            int cnt = 0;
            int bcpProcessesCnt = 0;
            int idx = CurrentJobInfo.BCPUploadCommand.IndexOf(CurrentJobInfo.TableName + "\" in \"");

            if (idx < 0)
            {
                StringBuilder tempOutputFile = new StringBuilder(CommonFunc.GetAppSettingsStringValue("BCPFileDir"));
                if (!tempOutputFile.ToString().EndsWith("\\"))
                {
                    tempOutputFile.Append("\\");
                }
                bcpOutputFileBase = tempOutputFile.Append(CurrentJobInfo.TableName).ToString();
            }
            else
            {
                int begIdx = idx + CurrentJobInfo.TableName.Length + 6;
                int endIdx = CurrentJobInfo.BCPUploadCommand.Substring(begIdx).IndexOf("\"");
                bcpOutputFileBase = CurrentJobInfo.BCPUploadCommand.Substring(begIdx, endIdx);
            }

            lock (_stringBuilderLock)
            {
                _bcpOutput.Remove(0, _bcpOutput.Length);
            }

            AsyncBCPJobEventArgs args = new AsyncBCPJobEventArgs((TabPage)Parent, CurrentJobInfo.JobStatus, rtbResults, "", Color.DarkSlateBlue);

            args.CurrentThreadIndex = CurrentJobInfo.CurrentThreadIndex;

            CurrentJobInfo.JobStatus = CommandStatus.InProcess;
            args.ClearRTB = true;

            NameValueCollection englishLanguage = (NameValueCollection)ConfigurationManager.GetSection("en-US");
            NameValueCollection defaultLanguage = (NameValueCollection)ConfigurationManager.GetSection(Thread.CurrentThread.CurrentCulture.Name);
            if (defaultLanguage == null)
            {
                defaultLanguage = englishLanguage;
                if (defaultLanguage == null)
                {
                    args.DisplayText = CommonFunc.FormatString(Properties.Resources.ErrorLanguageSectionNotFound, Application.ExecutablePath);
                    args.DisplayColor = Color.Red;
                    args.Status = CurrentJobInfo.JobStatus = CommandStatus.Failed;
                    UpdateBCPJobStatus(args);
                    return;
                }
            }

            string uploadTo = CurrentJobInfo.TableName;

            args.DisplayText = DateTime.Now.ToString(CultureInfo.CurrentCulture) + Environment.NewLine + CommonFunc.FormatString(Properties.Resources.BCPUploadingData, uploadTo, CurrentJobInfo.BCPUploadCommand) + Environment.NewLine;
            args.DisplayColor = Color.DarkSlateBlue;
            UpdateBCPJobStatus(args);
            args.ClearRTB = false;

            for (long i = 0; i < (CurrentJobInfo.NumberOfRows / chunkSize) + 1; i++)
            {
                string errorMsg = "";
                bool retry = true;
                int retryCnt = 0;
                long start = i * chunkSize + 1;
                long end = Math.Min((i + 1) * chunkSize, CurrentJobInfo.NumberOfRows);

                // Don't sleep on first chunk.
                if (i > 0)
                {
                    Thread.Sleep(sleep);
                }

                int endOfBCPCmd = CurrentJobInfo.BCPUploadCommand.IndexOf(ConfigurationManager.AppSettings["BCPExe"] + " ");
                if (endOfBCPCmd > -1)
                {
                    endOfBCPCmd = ConfigurationManager.AppSettings["BCPExe"].Length;
                }
                else
                {
                    System.Text.RegularExpressions.Match bcpExe = Regex.Match(CurrentJobInfo.BCPUploadCommand, @"\s[""[]");
                    if (bcpExe.Success)
                    {
                        endOfBCPCmd = bcpExe.Index;
                    }
                    else
                    {
                        endOfBCPCmd = CurrentJobInfo.BCPUploadCommand.IndexOf(" ");
                    }
                }

                while (retry && start <= end)
                {
                    if (++bcpProcessesCnt == 1)
                    {
                        bcpOutputFile = bcpOutputFileBase + ".txt";
                    }
                    else
                    {
                        bcpOutputFile = bcpOutputFileBase + "_" + bcpProcessesCnt.ToString() + ".txt";
                    }

                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = CurrentJobInfo.BCPUploadCommand.Substring(0, endOfBCPCmd);
                        if (CurrentJobInfo.NumberOfRows < chunkSize)
                        {
                            p.StartInfo.Arguments = CurrentJobInfo.BCPUploadCommand.Substring(endOfBCPCmd + 1);
                        }
                        else
                        {
                            p.StartInfo.Arguments = CurrentJobInfo.BCPUploadCommand.Substring(endOfBCPCmd + 1) + @" -F" + start.ToString(CultureInfo.InvariantCulture) + @" -L" + end.ToString(CultureInfo.InvariantCulture);
                        }

                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(defaultLanguage["BCPCodePage"]);
                        p.SynchronizingObject = this;
                        p.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputHandler);
                        p.Start();
                        p.BeginOutputReadLine();

                        while (!p.HasExited)
                        {
                            if (AsyncProcessingStatus.CancelProcessing)
                            {
                                p.Kill();
                                p.Close();
                                CurrentJobInfo.JobStatus = CommandStatus.Failed;
                                return;
                            }

                            // This is just to give the user something to look at so that they know we are processing something.

                            if (cnt++ > 80)
                            {
                                args.DisplayText = "*" + Environment.NewLine;
                                UpdateBCPJobStatus(args);
                                cnt = 0;
                            }
                            else
                            {
                                args.DisplayColor = args.DisplayColor == Color.Red ? Color.DarkSlateBlue : Color.Red;
                                args.DisplayText = "*";
                                UpdateBCPJobStatus(args);
                            }
                            p.WaitForExit(1000);
                        }
                        p.WaitForExit();
                        p.Close();
                    }
                    args.DisplayColor = Color.DarkSlateBlue;

                    if (File.Exists(bcpOutputFile))
                    {
                        File.Delete(bcpOutputFile);
                    }

                    File.WriteAllText(bcpOutputFile, GetThreadSafeBCPOutputString());
                    args.DisplayText = Environment.NewLine + CommonFunc.FormatString(Properties.Resources.BCPOutputFile) + bcpOutputFile;
                    args.DisplayColor = Color.DarkSlateBlue;
                    UpdateBCPJobStatus(args);

                    // Look for error.

                    Match error = Regex.Match(GetThreadSafeBCPOutputString(), defaultLanguage["BCPSQLState"]);
                    if (!error.Success)
                    {
                        error = Regex.Match(GetThreadSafeBCPOutputString(), englishLanguage["BCPSQLState"]);
                    }

                    if (error.Success)
                    {
                        // Check for Connection-Loss Errors.  If it is not a connection loss error, then don't bother to retry

                        if (!Regex.IsMatch(GetThreadSafeBCPOutputString(), CommonFunc.GetAppSettingsStringValue("BCPSQLAzureErrorCodesRetry")))
                        {
                            retry = false;
                            args.DisplayColor = Color.Red;
                            args.DisplayText = Environment.NewLine + DateTime.Now.ToString(CultureInfo.CurrentCulture) + Properties.Resources.Error1 + Environment.NewLine + Environment.NewLine + GetThreadSafeBCPOutputString() + Environment.NewLine;
                            args.Status = CurrentJobInfo.JobStatus = CommandStatus.Failed;
                            UpdateBCPJobStatus(args);
                            continue;
                        }

                        if (retryCnt == 0)
                        {
                            // Save off origial error message

                            errorMsg = GetThreadSafeBCPOutputString();
                        }

                        // Ok, found error.  Lets see if any data was sent

                        // Example Error
                        //  5000 rows sent to SQL Server. Total sent: 155000
                        //  SQLState = S1000, NativeError = 21
                        //  Error = [Microsoft][SQL Server Native Client 10.0][SQL Server]Warning: Fatal error 40501 occurred at Oct 30 2009  4:15PM. Note the error and time, and contact your system administrator.
                        //  BCP copy in failed

                        Match sent = Regex.Match(GetThreadSafeBCPOutputString(), defaultLanguage["BCPTotalSent"]);
                        if (sent.Success)
                        {
                            Match recordsSent = Regex.Match(sent.Value, defaultLanguage["BCPNumber"]);
                            if (recordsSent.Success)
                            {
                                int totalSent = Convert.ToInt32(recordsSent.Value, CultureInfo.InvariantCulture);  // Ok, we had data sent.  Get the number of rows sent.
                                start += totalSent;                                  // Now, we need to ofset start to start after last successful batch
                                rowsCopied += totalSent;                             // Add totalSent this batch to rows copied so far.
                            }
                        }
                        else
                        {
                            sent = Regex.Match(GetThreadSafeBCPOutputString(), englishLanguage["BCPTotalSent"]);
                            if (sent.Success)
                            {
                                Match recordsSent = Regex.Match(sent.Value, englishLanguage["BCPNumber"]);
                                if (recordsSent.Success)
                                {
                                    int totalSent = Convert.ToInt32(recordsSent.Value, CultureInfo.InvariantCulture);  // Ok, we had data sent.  Get the number of rows sent.
                                    start += totalSent;                                  // Now, we need to ofset start to start after last successful batch
                                    rowsCopied += totalSent;                             // Add totalSent this batch to rows copied so far.
                                }
                            }
                        }

                        if (retryCnt++ > 2)
                        {
                            // Give up
                            retry = false;
                            args.DisplayColor = Color.Red;
                            args.Status = CurrentJobInfo.JobStatus = CommandStatus.Failed;
                            args.DisplayText = Environment.NewLine + DateTime.Now.ToString(CultureInfo.CurrentCulture) + Properties.Resources.BCPProcessFailed + Environment.NewLine + Environment.NewLine + errorMsg + Environment.NewLine;
                            UpdateBCPJobStatus(args);
                        }
                        else
                        {
                            Thread.Sleep(sleep);
                        }
                    }
                    else
                    {
                        string bcpSum = Environment.NewLine;
                        Match summary = Regex.Match(GetThreadSafeBCPOutputString(), defaultLanguage["BCPSummary"]);
                        if (summary.Success)
                        {
                            bcpSum = Environment.NewLine + summary.Value + Environment.NewLine;
                        }
                        else
                        {
                            summary = Regex.Match(GetThreadSafeBCPOutputString(), englishLanguage["BCPSummary"]);
                            if (summary.Success)
                            {
                                bcpSum = Environment.NewLine + summary.Value + Environment.NewLine;
                            }
                        }

                        retry = false;
                        totalUploaded = end;

                        rowsCopied += end - start + 1;

                        args.DisplayText = Environment.NewLine + DateTime.Now.ToString(CultureInfo.CurrentCulture) + " --> " +
                                           string.Format(Properties.Resources.BCPProcessingUpdate + bcpSum,
                                           rowsCopied, CurrentJobInfo.NumberOfRows, decimal.Round((decimal)((float)rowsCopied / (float)CurrentJobInfo.NumberOfRows * 100.0), 2));
                        UpdateBCPJobStatus(args);
                    }

                    lock (_stringBuilderLock)
                    {
                        _bcpOutput.Length = 0;
                    }
                }
            }

            lock (_stringBuilderLock)
            {
                _bcpOutput.Length = 0;
            }

            args.DisplayText = Environment.NewLine;

            if (CurrentJobInfo.JobStatus == CommandStatus.InProcess)
            {
                args.Status = CurrentJobInfo.JobStatus = CommandStatus.Success;

                NotifyJobIsFinished();
            }

            UpdateBCPJobStatus(args);
        }
    }
}
