using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Trace;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using SQLAzureMWUtils;
using System.Globalization;
using System.Collections;

namespace SQLAzureMW
{
    /// <summary>
    /// This is the Azure SQL DB Migration Wizard.
    /// </summary>
    /// <devdoc>
    /// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    /// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
    /// PARTICULAR PURPOSE.
    /// </devdoc>
    /// <author name="George Huey" />
    /// <history>
    ///     <change date="9/9/2009" user="George Huey">
    ///         Added headers, etc.
    ///     </change>
    /// </history>

    public partial class ScriptWizard : Form, IMigrationOutput
    {
        private readonly BackgroundWorker _bckGndWkr = new BackgroundWorker();
        private StringBuilder rtbSQLScriptBuffer = new StringBuilder(5000);
        private Color rtbSQLScriptCurrentColor = Color.White;
        private SMOScriptOptions _smoScriptOpts;
        private Database _sourceDatabase;
        private TabPage _sqlResultTab;
        private Server _SourceServer;
        private TargetServerInfo _TargetServerInfo;
        private TargetServerInfo _SourceServerInfo;
        private string _FileToProcess;
        private System.IO.FileInfo[] _FilesToProcess;
        private System.IO.FileInfo[] _FilesModified;
        private DirectoryInfo _DirectoryToProcess;
        private DirectoryInfo _BeforeFilePath;
        private DirectoryInfo _AfterFilePath;
        private string _sqlForAzure;
        private bool _ParseFile;
        private int[] _crumb;
        private int _wizardIndex;
        private int _crumbIdx;
        private bool _Reset;
        private bool _IgnorCheck;
        private System.Threading.Thread _runningThread;
        private AsyncNotificationEventArgs _asyncEventArgs = null; //this is currently use for parsing folder; but can be used for other Wizard Processes
        private object _outputLock = new object();

        private string[] _CurrentWizardAction = new string[8];
        private string[] _CurrentWizardActDes = new string[8];

        public ScriptWizard()
        {
            //string culture = "af-ZA";
            //string culture = "de-DE";
            //string culture = "es-ES";
            //string culture = "fr-FR";
            //string culture = "it-IT";
            //string culture = "ja-JP";
            //string culture = "nl-nl";
            //string culture = "zh-CN";
            //string culture = "zh-TW";

            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
            //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

            _CurrentWizardAction[0] = CommonFunc.FormatString(Properties.Resources.WizardAction);
            _CurrentWizardAction[1] = CommonFunc.FormatString(Properties.Resources.WizardActionSelectSource);
            _CurrentWizardAction[2] = CommonFunc.FormatString(Properties.Resources.WizardActionObjectTypes);
            _CurrentWizardAction[3] = CommonFunc.FormatString(Properties.Resources.WizardActionScriptWizardSummary);
            _CurrentWizardAction[4] = CommonFunc.FormatString(Properties.Resources.WizardActionResultsSummary);
            _CurrentWizardAction[5] = CommonFunc.FormatString(Properties.Resources.WizardActionSetupTargetConnection);
            _CurrentWizardAction[6] = CommonFunc.FormatString(Properties.Resources.WizardActionTargetResults);
            _CurrentWizardAction[7] = CommonFunc.FormatString(Properties.Resources.WizardActionOverwriteFiles);

            _CurrentWizardActDes[0] = CommonFunc.FormatString(Properties.Resources.WizardActionDesc);
            _CurrentWizardActDes[1] = CommonFunc.FormatString(Properties.Resources.WizardActionSelectSourceDesc);
            _CurrentWizardActDes[2] = CommonFunc.FormatString(Properties.Resources.WizardActionObjectTypesDesc);
            _CurrentWizardActDes[3] = CommonFunc.FormatString(Properties.Resources.WizardActionScriptWizardSummaryDesc);
            _CurrentWizardActDes[4] = CommonFunc.FormatString(Properties.Resources.WizardActionResultsSummaryDesc);
            _CurrentWizardActDes[5] = CommonFunc.FormatString(Properties.Resources.WizardActionSetupTargetConnectionDesc);
            _CurrentWizardActDes[6] = CommonFunc.FormatString(Properties.Resources.WizardActionTargetResultsDesc);
            _CurrentWizardActDes[7] = CommonFunc.FormatString(Properties.Resources.WizardActionOverwriteFilesDesc);

            InitializeComponent();

            _bckGndWkr.DoWork += _bckGndWkr_DoWork;
            _bckGndWkr.RunWorkerCompleted += _bckGndWkr_RunWorkerCompleted;
            _bckGndWkr.WorkerSupportsCancellation = true;

            this.ClientSize = new System.Drawing.Size(600, 630);
            this.panel1.Size = new Size(595, 75);
            this.panel2.Size = new Size(595, 40);
            this.panel3.Size = new Size(595, 500);
            this.panel2.Location = new Point(5, panel1.Height + panel3.Height + 10);
            InitializeAll();
            DisplayNext();
            Initialize_BCPJobs();

            _smoScriptOpts = SMOScriptOptions.CreateFromConfig();
        }

        void _bckGndWkr_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                MessageBox.Show(CommonFunc.FormatString(Properties.Resources.DatabaseCreated, _TargetServerInfo.TargetDatabase));
                Retry.ExecuteRetryAction(() =>
                {
                    using (SqlConnection sqlConnection = new SqlConnection(_TargetServerInfo.ConnectionStringRootDatabase))
                    {
                        DisplayDatabases(sqlConnection, lbTargetDatabases);
                    }
                });
            }
            progressBarWaiting.Visible = false;
        }

        void _bckGndWkr_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }


                bool isOnline = CommonFunc.IsDBOnline(_TargetServerInfo);
                if (isOnline)
                {
                    return;
                }

                Thread.Sleep(5000);
            }
        }

        private void ScriptWizard_Load(object sender, EventArgs e)
        {
            this.Show();
            Application.DoEvents();
        }

        private void CancelAsyncProcesses()
        {
            AsyncProcessingStatus.FinishedProcessingJobs = true;

            if (_ThreadManager != null)
            {
                _ThreadManager.Abort();
                _ThreadManager = null;
            }

            if (_runningThread != null)
            {
                _runningThread.Abort();
                _runningThread = null;
            }
        }

        private void StartThread(ThreadStart ts)
        {
            if (_runningThread != null)
            {
                _runningThread.Abort();
            }

            _runningThread = new System.Threading.Thread(ts);
            _runningThread.CurrentCulture = CultureInfo.CurrentCulture;
            _runningThread.CurrentUICulture = CultureInfo.CurrentUICulture;
            _runningThread.Start();
        }

        public void rtbSQLScriptAppendText(string text, Color textColor)
        {
            if (rtbSQLScriptCurrentColor == Color.White)
            {
                rtbSQLScriptCurrentColor = textColor;
            }

            if (rtbSQLScriptCurrentColor == textColor)
            {
                rtbSQLScriptBuffer.Append(text);
            }
            else
            {
                AppendText(rtbSQLScript, rtbSQLScriptBuffer.ToString(), rtbSQLScriptCurrentColor);
                rtbSQLScriptBuffer.Length = 0;
                rtbSQLScriptCurrentColor = textColor;
                rtbSQLScriptBuffer.Append(text);
            }
        }

        private void AppendText(RichTextBox rtb, string text, Color selectionColor)
        {
            //if (this.InvokeRequired)
            //{
            //    this.Invoke(new MethodInvoker(delegate
            //    {
            //        AppendText(rtb, text, selectionColor);
            //    }));
            //    return;
            //}
            rtb.SuspendLayout();
            rtb.SelectionColor = selectionColor;
            rtb.AppendText(text);
            if (cbResultsScroll.Checked)
            {
                rtb.ScrollToCaret();
            }
            rtb.ResumeLayout();
        }

        public void StatusUpdateHandler(AsyncNotificationEventArgs args)
        {
            if (this.InvokeRequired)
            {
                AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(StatusUpdateHandler);
                this.Invoke(updateStatus, new object[] { args });
                return;
            }

            //lock (_outputLock)
            {
                if (args.FunctionCode == NotificationEventFunctionCode.SqlOutput)
                {
                    rtbSQLScriptAppendText(args.DisplayText, args.DisplayColor);
                }
                else if (args.FunctionCode == NotificationEventFunctionCode.AnalysisOutput)
                {
                    AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                }
                else if (args.FunctionCode == NotificationEventFunctionCode.ParseFolder && args.PercentComplete < 100)
                {
                    args.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingStatus, args.NumeratorComplete.ToString(CultureInfo.CurrentUICulture), args.TotalDenominator.ToString(CultureInfo.CurrentUICulture),
                        args.FilesProcessed.ToString(CultureInfo.CurrentUICulture), args.FilesToProcess.ToString(CultureInfo.CurrentUICulture));
                    progressBarGenScript.Value = args.PercentComplete;
                    lbResultsSummaryStatus.Text = args.StatusMsg;

                    if (args.DisplayText.Length > 0)
                    {
                        AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                    }
                }
                else
                {
                    progressBarGenScript.Value = args.PercentComplete;
                    lbResultsSummaryStatus.Text = args.StatusMsg;

                    if (args.DisplayText.Length > 0)
                    {
                        AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                    }
                }

                if (args.PercentComplete == 100)
                {
                    rtbSQLScriptAppendText("", Color.White); // Flush rtbSQLScript buffer
                    rtbResultsSummary.ScrollToCaret();
                    btnSave.Enabled = true;
                    btnBack.Enabled = true;
                    if (AsyncProcessingStatus.CancelProcessing || !_smoScriptOpts.Migrate)
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }
                }
            }
        }

        private void StartFileParseAsyncProcess(string file, bool parse)
        {
            System.Threading.ThreadStart ts = null;
            rtbSQLScript.Clear();
            rtbSQLScriptCurrentColor = Color.White;
            rtbSQLScriptBuffer.Length = 0;

            rtbResultsSummary.Clear();

            btnBack.Enabled = false;
            btnSave.Enabled = false;
            progressBarGenScript.Value = 0;
            progressBarGenScript.Visible = true;
            lbResultsSummaryStatus.Text = "";
            lbResultsSummaryStatus.Visible = true;
            _FileToProcess = file;

            if (rbAnalyzeTraceFile.Checked)
            {
                tabCtlResults.TabPages.Remove(_sqlResultTab);
                ts = new System.Threading.ThreadStart(ParseTraceFile);
            }
            else
            {
                if (tabCtlResults.TabPages.Count == 1)
                {
                    tabCtlResults.TabPages.Add(_sqlResultTab);
                }

                _ParseFile = parse;

                ts = new System.Threading.ThreadStart(RunFileParsingOnNewThread);
            }
            StartThread(ts);
        }

        private void StartFolderParseAsyncProcess()
        {
            tabCtlResults.TabPages.Remove(_sqlResultTab);
            rtbResultsSummary.Clear();
            btnBack.Enabled = true;
            btnSave.Enabled = false;
            progressBarGenScript.Value = 0;
            progressBarGenScript.Visible = true;
            lbResultsSummaryStatus.Text = "";
            lbResultsSummaryStatus.Visible = true;
            _ParseFile = true;

            _asyncEventArgs = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ParseFolder, 0, "", "", Color.Black);
            _asyncEventArgs.FilesToProcess = _FilesToProcess.Count();
            _asyncEventArgs.DisplayColor = Color.DarkBlue;
            _asyncEventArgs.DisplayText = "";
            _asyncEventArgs.NumeratorComplete = 0;
            _asyncEventArgs.TotalDenominator = 0;
            _asyncEventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingStatus, _asyncEventArgs.NumeratorComplete.ToString(CultureInfo.CurrentUICulture), _asyncEventArgs.TotalDenominator.ToString(CultureInfo.CurrentUICulture),
                _asyncEventArgs.FilesProcessed.ToString(CultureInfo.CurrentUICulture), _asyncEventArgs.FilesToProcess.ToString(CultureInfo.CurrentUICulture));
            _asyncEventArgs.PercentComplete = 0;
            StatusUpdateHandler(_asyncEventArgs);

            Thread ts = new Thread(() => RunFolderParsingOnNewThread());
            ts.Start();
        }

        private void CopyFileFromOriginalToTempDirectory(FileInfo f)
        {
            string targetDirectoryPath = _BeforeFilePath.FullName + f.DirectoryName.Remove(0, _DirectoryToProcess.FullName.Trim('\\').Length);
            if (!Directory.Exists(targetDirectoryPath))
            {
                Directory.CreateDirectory(targetDirectoryPath);
            }
            string copyFile = string.Concat(targetDirectoryPath, "\\", f.Name);
            if (!File.Exists(copyFile))
            {
                File.Copy(f.FullName, copyFile);
            }
        }

        public string CreateFilePathToTempDirectory(FileInfo f)
        {
            string targetDirectoryPath = _AfterFilePath.FullName + f.DirectoryName.Remove(0, _DirectoryToProcess.FullName.Trim('\\').Length);
            String file = String.Concat(targetDirectoryPath, "\\", f.Name);
            if (!Directory.Exists(targetDirectoryPath))
            {
                Directory.CreateDirectory(targetDirectoryPath);
            }
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            return file;
        }

        private void CreateBeforeAndAfterDirectories()
        {
            string[] RootDirectorySplit = tbSourceFile.Text.Split('\\');
            int indexOfRootFile = RootDirectorySplit.Length - 1;

            string beforeFilePath = System.IO.Path.GetTempPath() + @"SAMW_before_" + RootDirectorySplit[indexOfRootFile].ToString();
            string afterFilePath = System.IO.Path.GetTempPath() + @"SAMW_after_" + RootDirectorySplit[indexOfRootFile].ToString();
            _BeforeFilePath = new DirectoryInfo(beforeFilePath);
            _AfterFilePath = new DirectoryInfo(afterFilePath);
            if (!Directory.Exists(beforeFilePath))
            {
                _BeforeFilePath.Create();
            }

            if (!Directory.Exists(afterFilePath))
            {
                _AfterFilePath.Create();
            }
        }

        private void RunFolderParsingOnNewThread()
        {
            DateTime dtStart = DateTime.Now;

            _asyncEventArgs.PercentComplete = 0;
            _asyncEventArgs.FilesProcessed = 0;
            int numFilesToProcess = _FilesToProcess.Count();
            //create a subdirectory in the temp directory
            CreateBeforeAndAfterDirectories();

            List<Thread> threadPool = new List<Thread>();

            for (int i = 0; i < _FilesToProcess.Length; i++)
            {
                //we do not create file here yet; we will create when we create streamwriter.
                System.IO.FileInfo temp = _FilesToProcess.ElementAt(i);
                if (!temp.Extension.Equals(".sql", StringComparison.OrdinalIgnoreCase))
                {
                    --numFilesToProcess;
                    continue;
                }
                string targetFile = CreateFilePathToTempDirectory(temp);
                CopyFileFromOriginalToTempDirectory(temp);

                while (threadPool.Count > 20)
                {
                    Thread.Sleep(100);
                    for (int poolIdx = 0; poolIdx < threadPool.Count; poolIdx++)
                    {
                        if (threadPool[poolIdx].ThreadState == System.Threading.ThreadState.Stopped)
                        {
                            threadPool.RemoveAt(poolIdx);
                            break;
                        }
                    }
                }

                var migrator = new TsqlFileMigrator(temp.FullName, i, targetFile, this);

                //ThreadStart ts = new System.Threading.ThreadStart(delegate() { WorkerThread(migrator); });
                //Thread wt = new Thread(ts);
                //wt.Start();
                //threadPool.Add(wt);

                WorkerThread(migrator);
                //ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(WorkerThread), migrator);
            }

            while (threadPool.Count > 0)
            {
                for (int poolIdx = 0; poolIdx < threadPool.Count; poolIdx++)
                {
                    if (threadPool[poolIdx].ThreadState == System.Threading.ThreadState.Stopped)
                    {
                        threadPool.RemoveAt(poolIdx);
                        break;
                    }
                }
            }

            while (_asyncEventArgs.PercentComplete < 100)
            {
                if (_asyncEventArgs.FilesProcessed == numFilesToProcess)
                {
                    DateTime dtEnd = DateTime.Now;
                    string elapsedTime = GetElapsedTime(dtStart);
                    _asyncEventArgs.StatusMsg = Properties.Resources.Done;
                    _asyncEventArgs.PercentComplete = 100;
                    _asyncEventArgs.DisplayColor = Color.DarkCyan;
                    _asyncEventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageFinishedWithAnalysis, dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), elapsedTime);
                }
                else if (AsyncProcessingStatus.CancelProcessing)
                {
                    _asyncEventArgs.StatusMsg = Properties.Resources.MessageCanceled;
                    _asyncEventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageCanceledProcessing, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
                    _asyncEventArgs.DisplayColor = Color.DarkCyan;
                    _asyncEventArgs.PercentComplete = 100;
                }
                else if (_asyncEventArgs.TotalDenominator > 0)
                {
                    _asyncEventArgs.PercentComplete = _asyncEventArgs.NumeratorComplete * 100 / _asyncEventArgs.TotalDenominator;
                    _asyncEventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingStatus, _asyncEventArgs.NumeratorComplete.ToString(CultureInfo.CurrentUICulture), _asyncEventArgs.TotalDenominator.ToString(CultureInfo.CurrentUICulture),
                        _asyncEventArgs.FilesProcessed.ToString(CultureInfo.CurrentUICulture), _asyncEventArgs.FilesToProcess.ToString(CultureInfo.CurrentUICulture));
                    _asyncEventArgs.DisplayText = "";
                    Thread.Sleep(3000);
                }
                StatusUpdateHandler(_asyncEventArgs);
            }
        }

        private void WorkerThread(Object migrator)
        {
            TsqlFileMigrator ts = (TsqlFileMigrator)migrator;
            bool changed = ts.ParseFile(_smoScriptOpts, true, _asyncEventArgs);
            if (changed)
            {
                _FilesModified[ts._FileIndex] = new FileInfo(ts._TargetFile);
            }
        }

        private void RunFileParsingOnNewThread()
        {
            var migrator = new TsqlFileMigrator(_FileToProcess, this);
            AsyncNotificationEventArgs e = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ParseFile, 0, "", "", Color.Black);
            migrator.ParseFile(_smoScriptOpts, _ParseFile, e);
        }

        private void GenScriptAsyncUpdateStatusHandler(AsyncNotificationEventArgs args)
        {
            if (this.InvokeRequired)
            {
                AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(GenScriptAsyncUpdateStatusHandler);
                this.Invoke(updateStatus, new object[] { args });
            }
            else
            {
                if (args.FunctionCode == NotificationEventFunctionCode.AnalysisOutput)
                {
                    AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                }
                else if (args.FunctionCode == NotificationEventFunctionCode.SqlOutput)
                {
                    rtbSQLScriptAppendText(args.DisplayText, args.DisplayColor);
                }
                else
                {
                    progressBarGenScript.Value = args.PercentComplete;
                    lbResultsSummaryStatus.Text = args.StatusMsg;

                    if (args.DisplayText.Length > 0)
                    {
                        AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                    }

                    if (args.PercentComplete == 100)
                    {
                        rtbSQLScriptAppendText("", Color.White); // Flush rtbSQLScript buffer
                        rtbResultsSummary.ScrollToCaret();
                        if (_smoScriptOpts.Migrate)
                        {
                            btnNext.Enabled = true;
                        }
                        btnBack.Enabled = true;
                        btnSave.Enabled = true;
                    }

                    if (args.DisableNext) btnNext.Enabled = false;
                }
            }
        }

        private Database GetSourceDatabaseFromListBox()
        {
            DatabaseInfo di = (DatabaseInfo)lbDatabases.SelectedItem;
            return di.DatabaseObject;
        }

        private void StartGenScriptAsyncProcess()
        {
            AsyncProcessingStatus.CancelProcessing = false;
            _sourceDatabase = GetSourceDatabaseFromListBox();
            rtbResultsSummary.Clear();
            rtbSQLScript.Clear();
            rtbSQLScriptBuffer.Length = 0;
            rtbSQLScriptCurrentColor = Color.White;
            btnBack.Enabled = false;
            btnSave.Enabled = false;
            progressBarGenScript.Value = 0;
            progressBarGenScript.Visible = true;
            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(GenerateScriptFromSourceServer);
            StartThread(ts);
        }

        private void TargetAsyncUpdateStatusHandler(AsyncNotificationEventArgs e)
        {
            if (this.InvokeRequired)
            {
                AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(TargetAsyncUpdateStatusHandler);
                this.Invoke(updateStatus, new object[] { e });
            }
            else
            {
                Color selectionColor = e.DisplayColor;
                if (e.FunctionCode == NotificationEventFunctionCode.BcpUploadData)
                {
                    if (e.PercentComplete == 0)
                    {
                        rtbTargetResults.AppendText(Environment.NewLine);
                    }
                    else if (e.PercentComplete != 100)
                    {
                        if (rtbTargetResults.SelectionColor == Color.DarkBlue)
                        {
                            selectionColor = Color.DarkRed;
                        }
                        else
                        {
                            selectionColor = Color.DarkBlue;
                        }
                    }
                }
                else if (e.FunctionCode == NotificationEventFunctionCode.ExecuteSqlOnAzure)
                {
                    progressBarTargetServer.Value = e.PercentComplete > 100 ? 100 : e.PercentComplete;
                    lbStatus.Text = e.StatusMsg;

                    if (e.PercentComplete >= 100)
                    {
                        btnSaveTargetResults.Enabled = true;
                        btnBack.Enabled = true;
                    }
                }

                rtbTargetResults.SelectionColor = selectionColor;
                rtbTargetResults.AppendText(e.DisplayText);
                if (cbAzureStatusScroll.Checked)
                {
                    rtbTargetResults.ScrollToCaret();
                }
            }
        }

        private void StartTargetAsyncProcess()
        {
            rtbTargetResults.Clear();
            rtbTargetResults.SuspendLayout();
            _TargetServerInfo.TargetDatabase = lbTargetDatabases.SelectedItem.ToString().Replace("[", "").Replace("]", "");

            _sqlForAzure = rtbSQLScript.Text;

            btnBack.Enabled = false;
            progressBarTargetServer.Value = 0;
            lbStatus.Text = "";
            lbStatus.Visible = true;

            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(ExecuteSQLonTarget);
            StartThread(ts);
        }

        private void InitializeAll()
        {
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.lbProgram, Application.ProductName + " v" + Application.ProductVersion.Substring(0, Application.ProductVersion.Length));
            _wizardIndex = WizardSteps.SelectProcess;
            _crumbIdx = WizardSteps.SelectProcess;
            _Reset = true;
            _crumb = new int[_CurrentWizardAction.Length];

            gbSelectDatabase2.Text = gbSelectDatabase.Text = Properties.Resources.SelectDatabase;

            foreach (Control con in panel3.Controls)
            {
                con.Dock = DockStyle.Fill;
            }

            cbTargetServerType.DataSource = CommonFunc.GetTargetServerTypes();
            cbTargetServerType.DisplayMember = "value";

            string tgs = CommonFunc.GetAppSettingsStringValue("TargetServerType");
            foreach (KeyValuePair<string, string> serv in cbTargetServerType.Items)
            {
                if (serv.Key.Equals(tgs, StringComparison.OrdinalIgnoreCase))
                {
                    cbTargetServerType.SelectedItem = serv;
                    break;
                }
            }

            //tvDatabaseObjects.StateImageList = new ImageList();
            //tvDatabaseObjects.StateImageList.Images.Add(SystemIcons.Asterisk);
            //tvDatabaseObjects.StateImageList.Images.Add(SystemIcons.Exclamation);
            //tvDatabaseObjects.StateImageList.Images.Add(SystemIcons.Question);

            // lbDatabases.DrawMode = DrawMode.OwnerDrawFixed;
            lbDatabases.DrawItem += new System.Windows.Forms.DrawItemEventHandler(lbDatabases_DrawItem);

            btnNext.Enabled = false;
            btnSave.Enabled = false;

            _sqlResultTab = tabCtlResults.TabPages[1];
            HideAll();
        }

        private void HideAll()
        {
            foreach (Control con in panel3.Controls)
            {
                con.Visible = false;
            }
        }

        //private TreeNode GetEncryptedNode(string nodeText)
        //{
        //    TreeNode tn = new TreeNode(CommonFunc.FormatString(Properties.Resources.Encrypted, nodeText));
        //    tn.ForeColor = Color.Red;
        //    tn.BackColor = Color.Yellow;
        //    return tn;
        //}

        private void FigureOutObjectTypesAvailable()
        {
            ResetFields(panel3);
            _Reset = false;

            tvDatabaseObjects.SuspendLayout();
            tvDatabaseObjects.Nodes.Clear();

            DatabaseInfo di = (DatabaseInfo)lbDatabases.SelectedItem;
            Database db = GetSourceDatabaseFromListBox();

            try
            {
                // Roles
                if (db.Roles.Count > 0)
                {
                    TreeNode tnRoles = null;
                    foreach (DatabaseRole role in db.Roles)
                    {
                        switch (role.Name)
                        {
                            case "db_accessadmin":
                            case "db_backupoperator":
                            case "db_datareader":
                            case "db_datawriter":
                            case "db_ddladmin":
                            case "db_denydatareader":
                            case "db_denydatawriter":
                            case "db_owner":
                            case "db_securityadmin":
                            case "public":
                                break;

                            default:
                                if (tnRoles == null)
                                {
                                    tnRoles = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeRoles);
                                }
                                tnRoles.Nodes.Add(role.ToString());
                                break;
                        }
                    }
                }

                // Schemas
                if (db.Schemas.Count > 0)
                {
                    TreeNode tnSchemas = null;
                    foreach (Schema sch in db.Schemas)
                    {
                        switch (sch.Name)
                        {
                            case "sys":
                            case "INFORMATION_SCHEMA":
                            case "guest":
                            case "dbo":
                            case "db_securityadmin":
                            case "db_owner":
                            case "db_denydatawriter":
                            case "db_denydatareader":
                            case "db_ddladmin":
                            case "db_datawriter":
                            case "db_datareader":
                            case "db_backupoperator":
                            case "db_accessadmin":
                                break;

                            default:
                                if (tnSchemas == null)
                                {
                                    tnSchemas = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeSchemas);
                                }
                                tnSchemas.Nodes.Add(sch.ToString());
                                break;
                        }
                    }
                }

                try
                {
                    if (db.Parent.ResourceVersion.Major > 9)
                    {
                        if (db.Assemblies.Count > 0)
                        {
                            TreeNode tnAssemblies = null;
                            foreach (SqlAssembly asm in db.Assemblies)
                            {
                                if (asm.IsSystemObject) continue;

                                if (tnAssemblies == null)
                                {
                                    tnAssemblies = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeSQLAssemblies);
                                }
                                tnAssemblies.Nodes.Add(asm.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

                try
                {
                    if (db.PartitionFunctions.Count > 0)
                    {
                        TreeNode tnPartitionFunc = null;
                        foreach (PartitionFunction pf in db.PartitionFunctions)
                        {
                            if (tnPartitionFunc == null)
                            {
                                tnPartitionFunc = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypePartitionFunctions);
                            }

                            tnPartitionFunc.Nodes.Add(pf.ToString());
                        }
                    }

                    if (db.PartitionSchemes.Count > 0)
                    {
                        TreeNode tnPartitionSch = null;
                        foreach (PartitionScheme ps in db.PartitionSchemes)
                        {
                            if (tnPartitionSch == null)
                            {
                                tnPartitionSch = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypePartitionSchemes);
                            }

                            tnPartitionSch.Nodes.Add(ps.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                // Stored Procedures
                if (db.StoredProcedures.Count > 0)
                {
                    TreeNode tnSP = null;
                    foreach (StoredProcedure sp in db.StoredProcedures)
                    {
                        if (sp.IsSystemObject) continue;

                        if (tnSP == null)
                        {
                            tnSP = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeStoredProcedures);
                        }

                        tnSP.Nodes.Add(sp.ToString());
                    }
                }

                if (db.Synonyms.Count > 0)
                {
                    TreeNode tnSynonyms = null;
                    foreach (Synonym syn in db.Synonyms)
                    {
                        if (tnSynonyms == null)
                        {
                            tnSynonyms = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeSynonyms);
                        }
                        tnSynonyms.Nodes.Add(syn.ToString());
                    }
                }

                try
                {
                    if (db.UserDefinedTableTypes.Count > 0)
                    {
                        TreeNode tnTableTypes = null;
                        foreach (UserDefinedTableType tt in db.UserDefinedTableTypes)
                        {
                            if (tnTableTypes == null)
                            {
                                tnTableTypes = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeUDTT);
                            }
                            tnTableTypes.Nodes.Add(tt.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                // Tables
                if (db.Tables.Count > 0)
                {
                    TreeNode tnTables = null;
                    foreach (Table tb in db.Tables)
                    {
                        if (tb.IsSystemObject) continue;

                        if (tnTables == null)
                        {
                            tnTables = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeTables);
                        }
                        tnTables.Nodes.Add(tb.ToString());
                    }
                }

                // Database Triggers
                if (db.Triggers.Count > 0)
                {
                    TreeNode tnTriggers = null;
                    foreach (DatabaseDdlTrigger trig in db.Triggers)
                    {
                        if (!trig.IsSystemObject)
                        {
                            if (tnTriggers == null)
                            {
                                tnTriggers = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeTriggers);
                            }

                            tnTriggers.Nodes.Add(trig.ToString());
                        }
                    }
                }

                try
                {
                    // UDT
                    if (db.UserDefinedDataTypes.Count > 0)
                    {
                        TreeNode tnUDT = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeUDT);
                        foreach (UserDefinedDataType uddt in db.UserDefinedDataTypes)
                        {
                            tnUDT.Nodes.Add(uddt.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowException(this, ex);
                }

                // UDF
                if (db.UserDefinedFunctions.Count > 0)
                {
                    TreeNode tnUDF = null;
                    foreach (UserDefinedFunction udf in db.UserDefinedFunctions)
                    {
                        if (udf.IsSystemObject) continue;

                        if (tnUDF == null)
                        {
                            tnUDF = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeUDF);
                        }

                        tnUDF.Nodes.Add(udf.ToString());
                    }
                }

                // Views
                if (db.Views.Count > 0)
                {
                    TreeNode tnViews = null;
                    foreach (Microsoft.SqlServer.Management.Smo.View vw in db.Views)
                    {
                        if (vw.IsSystemObject) continue;

                        if (tnViews == null)
                        {
                            tnViews = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeViews);
                        }

                        tnViews.Nodes.Add(vw.ToString());
                    }
                }

                try
                {
                    // XML Schema Collections
                    if (db.XmlSchemaCollections.Count > 0)
                    {
                        TreeNode tnXMLSC = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeXMLSchemaCollections);
                        foreach (XmlSchemaCollection xsc in db.XmlSchemaCollections)
                        {
                            tnXMLSC.Nodes.Add(xsc.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
            }
            tvDatabaseObjects.ResumeLayout();
        }

        private void AddChildNode(ref TreeNode tnObjects, TreeNode parent)
        {
            if (parent.Checked)
            {
                TreeNode tnObj = tnObjects.Nodes.Add(parent.Text);

                foreach (TreeNode child in parent.Nodes)
                {
                    if (child.Checked)
                    {
                        tnObj.Nodes.Add(child.Text);
                    }
                }
            }
        }

        private void DisplaySummary()
        {
            tvSummary.Nodes.Clear();

            SMOScriptOptions scriptOptions = _smoScriptOpts;
            Database db = GetSourceDatabaseFromListBox();

            TreeNode tnDatabase = tvSummary.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeDatabase));
            TreeNode tnObjects = tvSummary.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeObjects));
            TreeNode tnOptions = tvSummary.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeOptions));

            tnDatabase.Nodes.Add(db.ToString());
            tnDatabase.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TargetServer, scriptOptions.TargetServer));

            TreeNode tnGen = tnOptions.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeGeneral));
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptHeaders) + scriptOptions.ScriptHeaders.ToString());
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptDefaults) + scriptOptions.ScriptDefaults.ToString());
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptExtendedProperties) + scriptOptions.ScriptExtendedProperties.ToString());
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeIfNotExists) + scriptOptions.IncludeIfNotExists.ToString());
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptDropCreate) + scriptOptions.ScriptDropCreate.ToString());

            TreeNode tnTblVwOpt = tnOptions.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeTableView));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptConstraints, scriptOptions.ScriptCheckConstraints));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptCollation, scriptOptions.ScriptCollation));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptData, scriptOptions.ScriptTableAndOrData));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptForeignKeys, scriptOptions.ScriptForeignKeys));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptIndexes, scriptOptions.ScriptIndexes));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptPrimaryKey, scriptOptions.ScriptPrimaryKeys));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptUniqueKeys, scriptOptions.ScriptUniqueKeys));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptUniqueKeys, scriptOptions.ScriptTableTriggers));

            TreeNode tnTSQLCompat = tnOptions.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeTSQLCompatibility));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecks, scriptOptions.CompatibilityChecks));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksActiveDirectory, scriptOptions.ActiveDirectorySP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksBackupRestore, scriptOptions.BackupandRestoreTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksCDC, scriptOptions.ChangeDataCapture));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksCDCT, scriptOptions.ChangeDataCaptureTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksEngineSP, scriptOptions.DatabaseEngineSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksMail, scriptOptions.DatabaseMailSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksMaintenance, scriptOptions.DatabaseMaintenancePlan));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksDataControls, scriptOptions.DataControl));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksDistributedQueries, scriptOptions.DistributedQueriesSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksFullTextSearch, scriptOptions.FullTextSearchSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksGeneralExtended, scriptOptions.GeneralExtendedSPs));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksGeneralTSQL, scriptOptions.GeneralTSQL));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksIntegrationServicesTable, scriptOptions.IntegrationServicesTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksLogShipping, scriptOptions.LogShipping));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksMetadataFunction, scriptOptions.MetadataFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksOLEAutomationSP, scriptOptions.OLEAutomationSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksOLEDBTable, scriptOptions.OLEDBTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksProfilerSP, scriptOptions.ProfilerSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksReplicationSP, scriptOptions.ReplicationSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksReplicationTable, scriptOptions.ReplicationTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksRowsetFunction, scriptOptions.RowsetFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSecurityFunction, scriptOptions.SecurityFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSecuritySP, scriptOptions.SecuritySP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSQLMailSP, scriptOptions.SQLMailSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSQLServerAgentSP, scriptOptions.SQLServerAgentSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSQLServerAgentTable, scriptOptions.SQLServerAgentTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSystemCatalogView, scriptOptions.SystemCatalogView));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSystemFunction, scriptOptions.SystemFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSystemStatisticalFunction, scriptOptions.SystemStatisticalFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksUnclassified, scriptOptions.Unclassified));

            foreach (TreeNode parent in tvDatabaseObjects.Nodes)
            {
                AddChildNode(ref tnObjects, parent);
            }

            tnDatabase.Expand();
            tnOptions.Expand();
            tnObjects.Expand();

            panelTreeViewSummary.Visible = true;
        }

        private void SetActionTitles()
        {
            lbAction.Text = _CurrentWizardAction[_wizardIndex];
            lbActionDesc.Text = _CurrentWizardActDes[_wizardIndex];
        }

        private void DisplayPrevious()
        {
            if (_wizardIndex == WizardSteps.ResultsSummary)
            {
                btnNext.Enabled = true;
                btnNext.Visible = true;
            }
            _wizardIndex = _crumb[--_crumbIdx];

            SetActionTitles();

            btnSelectAll.Enabled = false;
            btnClearAll.Enabled = false;

            HideAll();

            switch (_wizardIndex)
            {
                case WizardSteps.SelectProcess:
                    btnBack.Enabled = false;
                    btnNext.Enabled = true;
                    panelWizardOptions.Visible = true;
                    break;

                case WizardSteps.SelectDatabaseSource:
                    btnBack.Enabled = true;
                    panelDatabaseSource.Visible = true;
                    break;

                case WizardSteps.SelectObjectsToScript:
                    btnBack.Enabled = true;
                    panelObjectTypes.Visible = true;
                    break;

                case WizardSteps.ScriptWizardSummary:
                    btnNext.Enabled = true;
                    panelTreeViewSummary.Visible = true;
                    break;

                case WizardSteps.ResultsSummary:
                    panelResultsSummary.Visible = true;
                    btnOverwrite.Visible = false;
                    btnNext.Enabled = true;
                    btnNext.Visible = true;
                    break;

                case WizardSteps.SetupTargetConnection:
                    panelTargetDatabase.Visible = true;
                    btnNext.Enabled = true;
                    break;
                case WizardSteps.OverwriteFiles:
                    panelOverwriteFileList.Visible = true;
                    //btnOK.Enabled = true;
                    btnCancel.Enabled = true;
                    break;
            }
        }

        private void ReadSourceFile(string file)
        {
            if (tabCtlResults.TabPages.Count == 1)
            {
                tabCtlResults.TabPages.Add(_sqlResultTab);
            }
            tabCtlResults.SelectedIndex = 1;

            try
            {
                StartFileParseAsyncProcess(file, rbAnalyzeMigrateTSQLFile.Checked);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                rtbResultsSummary.SelectionColor = Color.Red;
                rtbResultsSummary.AppendText(ex.Message + Environment.NewLine);
            }
        }

        private void BrowseRootFolder()
        {
            if (tabCtlResults.TabPages.Count == 2)
            {
                tabCtlResults.TabPages.Remove(_sqlResultTab);
            }
            tabCtlResults.SelectedIndex = 0;
            try
            {
                //files in current directory
                _FilesToProcess = _DirectoryToProcess.GetFiles("*.sql", System.IO.SearchOption.AllDirectories);
                _FilesModified = new FileInfo[_FilesToProcess.Count()];
                StartFolderParseAsyncProcess();
            }
            // if we get System.IO.DirectoryNotFoundException: The UI validated this directory already. throw exception and let main() catch this

            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                rtbResultsSummary.SelectionColor = Color.Red;
                rtbResultsSummary.AppendText(ex.Message + Environment.NewLine);
            }
        }

        private void DisplayNext()
        {
            SetActionTitles();

            btnClearAll.Enabled = false;
            btnSelectAll.Enabled = false;
            btnCancelProcessing.Enabled = true;
            btnOverwrite.Visible = false;
            HideAll();

            switch (_wizardIndex)
            {
                case WizardSteps.SelectProcess:
                    btnBack.Enabled = false;
                    panelWizardOptions.Visible = true;
                    break;

                case WizardSteps.SelectDatabaseSource: // Script Options
                    _smoScriptOpts.TargetServer = ((KeyValuePair<string, string>)cbTargetServerType.SelectedItem).Value;
                    btnBack.Enabled = true;
                    panelDatabaseSource.Visible = true;
                    if (lbDatabases.Items.Count == 0)
                    {
                        btnConnectToServer_Click(null, null);
                    }
                    break;

                case WizardSteps.SelectObjectsToScript:
                    if (_Reset)
                    {
                        FigureOutObjectTypesAvailable();
                        rbScriptAll.Checked = true;
                        btnSelectAll_Click(null, null);
                        _Reset = false;
                    }

                    btnBack.Enabled = true;
                    panelObjectTypes.Visible = true;
                    break;

                case WizardSteps.ScriptWizardSummary:
                    DisplaySummary();
                    btnNext.Enabled = true;
                    break;

                case WizardSteps.ResultsSummary:
                    panelResultsSummary.Visible = true;
                    if (rbAnalyzeTraceFile.Checked || rbTSQLFolder.Checked)
                    {
                        tabCtlResults.TabPages.Remove(_sqlResultTab);
                        tabCtlResults.SelectedIndex = 0;
                    }
                    else
                    {
                        if (tabCtlResults.TabPages.Count == 1)
                        {
                            tabCtlResults.TabPages.Add(_sqlResultTab);
                        }
                        tabCtlResults.SelectedIndex = 0;
                    }
                    btnNext.Enabled = false;
                    break;

                case WizardSteps.SetupTargetConnection:
                    panelTargetDatabase.Visible = true;

                    if (_TargetServerInfo == null)
                    {
                        btnConnectTargetServer_Click(null, null);
                    }
                    if (rbMaintenance.Checked) btnNext.Enabled = false;
                    btnBack.Enabled = true;
                    break;

                case WizardSteps.TargetResults:
                    panelTargetResults.Visible = true;
                    btnNext.Enabled = false;
                    break;

                case WizardSteps.OverwriteFiles:
                    panelOverwriteFileList.Visible = true;
                    btnNext.Enabled = false;
                    btnCancel.Enabled = true;
                    btnNext.Enabled = false;
                    btnNext.Visible = false;
                    btnOverwrite.Visible = true;
                    break;
            }
        }

        private void btnOverwrite_Click(object sender, EventArgs e)
        {
            DialogResult dr3 = MessageBox.Show(Properties.Resources.WizardActionOverwriteFilesDailogConfirmation, Properties.Resources.WizardActionOverwriteFiles, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr3 == DialogResult.Yes)
            {
                this.Cursor = Cursors.WaitCursor;
                OverwriteSQLFiles();
                this.Cursor = Cursors.Default;
                btnOverwrite.Enabled = false;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (rbMaintenance.Checked)
            {
                _crumb[_crumbIdx++] = _wizardIndex;
                _wizardIndex = WizardSteps.SetupTargetConnection;
                DisplayNext();
                return;
            }
            else if (_wizardIndex == WizardSteps.SelectProcess && (rbRunTSQLFile.Checked || rbAnalyzeTraceFile.Checked || rbAnalyzeMigrateTSQLFile.Checked))
            {
                if (File.Exists(tbSourceFile.Text))
                {
                    _crumb[_crumbIdx++] = _wizardIndex;
                    _wizardIndex = WizardSteps.ResultsSummary;
                    DisplayNext();
                    _smoScriptOpts.TargetServer = ((KeyValuePair<string, string>)cbTargetServerType.SelectedItem).Value;
                    ReadSourceFile(tbSourceFile.Text);
                }
                else
                {
                    btnNext.Enabled = false;
                    MessageBox.Show(Properties.Resources.MessageFileDoesNotExists);
                    tbSourceFile.Focus();
                }
                return;
            }
            else if (_wizardIndex == WizardSteps.SelectProcess && (rbTSQLFolder.Checked))
            {
                _DirectoryToProcess = new DirectoryInfo(tbSourceFile.Text);
                if (_DirectoryToProcess.Exists)
                {
                    _crumb[_crumbIdx++] = _wizardIndex;
                    _wizardIndex = WizardSteps.ResultsSummary;
                    DisplayNext();
                    _smoScriptOpts.TargetServer = ((KeyValuePair<string, string>)cbTargetServerType.SelectedItem).Value;
                    BrowseRootFolder();
                    btnBack.Enabled = false;
                }
                else
                {
                    btnNext.Visible = false;
                    //TODO: need to globalize this resource string MessageFolderDoesNotExists
                    MessageBox.Show(Properties.Resources.MessageFolderDoesNotExists);
                    tbSourceFile.Focus();
                }
                return;
            }
            else if (_wizardIndex == WizardSteps.ResultsSummary && (rbTSQLFolder.Checked))
            {

                _crumb[_crumbIdx++] = _wizardIndex;
                _wizardIndex = WizardSteps.OverwriteFiles;
                DisplayNext();
                //display the files that have changed with current location and target location
                btnOverwrite.Visible = true;
                btnCancel.Enabled = true;
                btnBack.Enabled = true;
                DisplayChangedFilesLists();
                return;
            }
            else if (_wizardIndex == WizardSteps.ScriptWizardSummary || _wizardIndex == WizardSteps.SetupTargetConnection)
            {
                this.Cursor = Cursors.WaitCursor;

                switch (_wizardIndex)
                {
                    case WizardSteps.ScriptWizardSummary:
                        DialogResult dr = MessageBox.Show(Properties.Resources.MessageReadyToGenerate, Properties.Resources.TitleGenerateScript, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                        {
                            btnNext.Enabled = false;
                            _crumb[_crumbIdx++] = _wizardIndex++;
                            DisplayNext();
                            StartGenScriptAsyncProcess();
                        }
                        break;

                    case WizardSteps.SetupTargetConnection:
                        DialogResult dr2 = MessageBox.Show(Properties.Resources.MessageExecuteScript, Properties.Resources.TitleExecuteScript, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr2 == DialogResult.Yes)
                        {
                            btnNext.Enabled = false;
                            _crumb[_crumbIdx++] = _wizardIndex++;
                            DisplayNext();
                            StartTargetAsyncProcess();
                        }
                        break;
                }
                this.Cursor = Cursors.Default;
                return;
            }


            this.Cursor = Cursors.WaitCursor;

            _crumb[_crumbIdx++] = _wizardIndex++;
            DisplayNext();

            this.Cursor = Cursors.Default;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            CancelAsyncProcesses();
            DisplayPrevious();
            AsyncProcessingStatus.CancelProcessing = false;
        }

        private void lbDatabases_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            if (e != null && e.Index > -1)
            {
                e.DrawBackground();
                Brush myBrush = Brushes.Gray;

                DatabaseInfo db = (DatabaseInfo)lbDatabases.Items[e.Index];
                if (db.IsDbOwner) //IsDbAccessAdmin
                {
                    myBrush = Brushes.Black;
                }
                e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                e.DrawFocusRectangle();
            }
        }

        private bool IsDatabaseOwner(SqlConnection connection)
        {
            bool owner = false;
            string sqlQuery = "SELECT is_member(N'db_owner') AS [IsDbOwner]";

            Retry.ExecuteRetryAction(() =>
            {
                ScalarResults sr = SqlHelper.ExecuteScalar(connection, CommandType.Text, sqlQuery);
                owner = Convert.ToBoolean((int)sr.ExecuteScalarReturnValue);
            });
            return owner;
        }

        private void DisplayDatabases(SqlConnection connection, ListBox lb)
        {
            lb.Items.Clear();
            string database = connection.Database;

            if (database.Length > 0 && !database.Equals("master", StringComparison.OrdinalIgnoreCase))
            {
                DatabaseInfo dbi = new DatabaseInfo();
                dbi.DatabaseName = database;
                dbi.ConnectedTo = TypeOfConnection.UsingADO;
                dbi.IsDbOwner = IsDatabaseOwner(connection);
                lb.Items.Add(dbi);
            }
            else
            {
                string sqlQuery = "SELECT dtb.name AS [Name]" +
                                       " , CAST(case when dtb.name in ('master','model','msdb','tempdb') then 1 else dtb.is_distributor end AS bit) AS [IsSystemObject]" +
                                       " , dtb.is_read_only AS [ReadOnly]" +
                                  " FROM sys.databases AS dtb" +
                                 " ORDER BY[Name] ASC";

                // Note to self.  For some reason yet to be figured out, ExecuteReader throws an error when swapping between SQL Azure and SQL Azure Federations
                // Closing the connection and opening the connection again seems to fix the problem.

                if (connection.State != ConnectionState.Closed) connection.Close();

                string tmp = _TargetServerInfo.TargetDatabase;
                Retry.ExecuteRetryAction(() =>
                {
                    connection.Open();

                    using (SqlDataReader sdr = SqlHelper.ExecuteReader(connection, CommandType.Text, sqlQuery))
                    {
                        while (sdr.Read())
                        {
                            bool IsSystemObject = sdr.GetBoolean(1);
                            if (IsSystemObject) continue;

                            DatabaseInfo dbi = new DatabaseInfo();
                            dbi.DatabaseName = sdr.GetString(0);
                            bool ReadOnly = sdr.GetBoolean(2);
                            if (ReadOnly)
                            {
                                dbi.IsDbOwner = false;
                            }
                            else
                            {
                                _TargetServerInfo.TargetDatabase = dbi.DatabaseName;
                                //using (SqlConnection con = new SqlConnection(_TargetServerInfo.ConnectionStringTargetDatabase))
                                //{
                                //    dbi.IsDbOwner = IsDatabaseOwner(con);
                                //    con.Closdbi.IsDbOwnere();
                                //}
                                dbi.IsDbOwner = true;
                            }
                            lb.Items.Add(dbi);
                        }
                        sdr.Close();
                    }

                    connection.Close();
                }, () =>
                {
                    connection.Close();
                });
                _TargetServerInfo.TargetDatabase = tmp;
            }
        }

        private void DisplayDatabases(ref Server dbServer, ListBox lb)
        {
            try
            {
                // Clear control
                lb.Items.Clear();
                string database = dbServer.ConnectionContext.SqlConnectionObject.Database;

                if (database.Length > 0 && !database.Equals("master", StringComparison.OrdinalIgnoreCase))
                {
                    Server svr = CommonFunc.GetSmoServer(dbServer.ConnectionContext);
                    Database db = (Database)svr.Databases[database];
                    DatabaseInfo di = new DatabaseInfo();
                    di.DatabaseObject = db;
                    try
                    {
                        di.IsDbOwner = db.IsDbOwner;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        di.IsDbOwner = false;
                    }
                    lb.Items.Add(di);
                }
                else
                {
                    // Add database objects to combobox; the default ToString will display the database name
                    foreach (Database db in dbServer.Databases)
                    {
                        DatabaseInfo di = new DatabaseInfo();
                        di.DatabaseObject = db;
                        if (db.IsSystemObject == true) continue;
                        try
                        {
                            if (db.IsDbOwner) //IsDbAccessAdmin
                            {
                                di.IsDbOwner = true;
                            }
                            else
                            {
                                di.IsDbOwner = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            di.IsDbOwner = false;
                        }

                        lb.Items.Add(di);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
            }
        }

        private void ResetFields(Panel pan)
        {
            foreach (Control ctl in pan.Controls)
            {
                if (ctl.GetType() == typeof(CheckedListBox))
                {
                    ((CheckedListBox)ctl).Items.Clear();
                }
                else if (ctl.GetType() == typeof(TreeView))
                {
                    ((TreeView)ctl).Nodes.Clear();
                }
                else if (ctl.GetType() == typeof(RichTextBox))
                {
                    ((RichTextBox)ctl).Clear();
                }
                else if (ctl.GetType() == typeof(Panel))
                {
                    ResetFields((Panel)ctl);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            bool allowNext = false;

            _IgnorCheck = true;
            foreach (TreeNode tn in tvDatabaseObjects.Nodes)
            {
                int numChildSelected = 0;
                foreach (TreeNode child in tn.Nodes)
                {
                    if (child.ForeColor == Color.Red) continue;

                    child.Checked = true;
                    ++numChildSelected;
                }

                if (numChildSelected > 0)
                {
                    tn.Checked = true;
                    allowNext = true;
                }
            }
            _IgnorCheck = false;

            if (allowNext) btnNext.Enabled = true;
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            _IgnorCheck = true;
            foreach (TreeNode tn in tvDatabaseObjects.Nodes)
            {
                tn.Checked = false;
                foreach (TreeNode child in tn.Nodes)
                {
                    child.Checked = false;
                }
            }
            _IgnorCheck = false;
            btnNext.Enabled = false;
        }

        private void lbDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Reset = true;
            if (lbDatabases.SelectedItem != null)
            {
                DatabaseInfo db = (DatabaseInfo)lbDatabases.SelectedItem;
                try
                {
                    if (db.IsDbOwner)
                    {
                        btnNext.Enabled = true;
                    }
                    else
                    {
                        btnNext.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    btnNext.Enabled = false;
                }
                return;
            }
            else
            {
                btnNext.Enabled = false;
            }
        }

        private SqlSmoObject[] GetSortedObjects(Database sourceDatabase)
        {
            // Ok, we need to get all of the selected objects and put them into one array
            // so that we can get them sorted by dependency.

            bool dataOnly = Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase);
            List<SqlSmoObject> objectList = new List<SqlSmoObject>();

            foreach (TreeNode parent in tvDatabaseObjects.Nodes)
            {
                if (parent.Checked)
                {
                    if (!dataOnly && _SourceServer.ConnectionContext.ServerVersion.Major > 9 && parent.Text.Equals(Properties.Resources.ObjectTypeTriggers))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.Triggers)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    else if (!dataOnly && parent.Text.Equals(Properties.Resources.ObjectTypeStoredProcedures))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.StoredProcedures)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    else if (parent.Text.Equals(Properties.Resources.ObjectTypeTables))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.Tables)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    else if (!dataOnly && parent.Text.Equals(Properties.Resources.ObjectTypeUDF))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.UserDefinedFunctions)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    else if (!dataOnly && parent.Text.Equals(Properties.Resources.ObjectTypeViews))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.Views)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Sort(sourceDatabase, objectList.ToArray());
        }

        private string GetElapsedTime(DateTime dtStart)
        {
            DateTime dtEnd = DateTime.Now;
            TimeSpan tsDuration = dtEnd.Subtract(dtStart);
            string sHour = tsDuration.Hours == 1 ? Properties.Resources.MessageHour : Properties.Resources.MessageHours;
            string sMin = tsDuration.Minutes == 1 ? Properties.Resources.MessageMinute : Properties.Resources.MessageMinutes;
            string sSecs = tsDuration.Seconds == 1 ? Properties.Resources.MessageSecond : Properties.Resources.MessageSeconds;
            return tsDuration.Hours + sHour + tsDuration.Minutes.ToString(CultureInfo.CurrentUICulture) + sMin + tsDuration.Seconds.ToString(CultureInfo.CurrentUICulture) + sSecs;

        }
        private SqlSmoObject[] Sort(Database sourceDatabase, SqlSmoObject[] smoObjects)
        {
            if (smoObjects.Count() < 2) return smoObjects;

            DateTime dtStart = DateTime.Now;
            DependencyTree dt = null;
            DependencyWalker dw = new DependencyWalker(sourceDatabase.Parent);

            try
            {
                dt = dw.DiscoverDependencies(smoObjects, true);
                Debug.WriteLine("Just Finished Discover Dependencies: " + GetElapsedTime(dtStart));

            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
                return smoObjects;
            }

            SqlSmoObject[] sorted = new SqlSmoObject[smoObjects.Count()];
            int index = 0;

            dtStart = DateTime.Now;
            foreach (DependencyCollectionNode d in dw.WalkDependencies(dt))
            {
                if (d.Urn.Type.Equals("UnresolvedEntity")) continue;

                foreach (SqlSmoObject sso in smoObjects)
                {
                    if (sso.Urn.ToString().Equals(d.Urn, StringComparison.CurrentCultureIgnoreCase))
                    {
                        sorted[index++] = sso;
                        break;
                    }
                }
            }
            Debug.WriteLine("Just Finished WalkDependencies: " + GetElapsedTime(dtStart));
            return sorted;
        }

        private bool AddObjectsFromTreeView(ref List<SqlSmoObject> objectList, SmoCollectionBase collection, TreeNode parent, string objectType)
        {
            if (!parent.Text.Equals(objectType)) return false;

            foreach (SqlSmoObject obj in collection)
            {
                foreach (TreeNode child in parent.Nodes)
                {
                    if (child.Checked)
                    {
                        if (obj.ToString().Equals(child.Text))
                        {
                            objectList.Add(obj);
                        }
                    }
                }
            }
            return true;
        }

        private SqlSmoObject[] GetSMOObjectListFromTreeViewNode(string objectType)
        {
            List<SqlSmoObject> objectList = new List<SqlSmoObject>();
            foreach (TreeNode parent in tvDatabaseObjects.Nodes)
            {
                if (parent.Text.Equals(objectType))
                {
                    if (parent.Checked)
                    {
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.Assemblies, parent, Properties.Resources.ObjectTypeSQLAssemblies)) continue;
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.Roles, parent, Properties.Resources.ObjectTypeRoles)) continue;
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.Schemas, parent, Properties.Resources.ObjectTypeSchemas)) continue;
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.XmlSchemaCollections, parent, Properties.Resources.ObjectTypeXMLSchemaCollections)) continue;
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.Synonyms, parent, Properties.Resources.ObjectTypeSynonyms)) continue;
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.UserDefinedDataTypes, parent, Properties.Resources.ObjectTypeUDT)) continue;
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.Triggers, parent, Properties.Resources.ObjectTypeTriggers)) continue;
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.PartitionFunctions, parent, Properties.Resources.ObjectTypePartitionFunctions)) continue;
                        if (AddObjectsFromTreeView(ref objectList, _sourceDatabase.PartitionSchemes, parent, Properties.Resources.ObjectTypePartitionSchemes)) continue;
                        AddObjectsFromTreeView(ref objectList, _sourceDatabase.UserDefinedTableTypes, parent, Properties.Resources.ObjectTypeUDTT);
                    }
                    break;
                }
            }
            return objectList.ToArray();
        }

        private void ScriptDrops(SqlSmoObject[] sorted, SqlSmoObject[] smoAssemblies, SqlSmoObject[] smoSynonyms, SqlSmoObject[] smoRoles, SqlSmoObject[] smoSchemas, SqlSmoObject[] smoSchemasCols, SqlSmoObject[] smoUDTs, SqlSmoObject[] smoUDTTs, SqlSmoObject[] smoPartitionFunctions, SqlSmoObject[] smoPartitionSchemas, ScriptDatabase sdb)
        {
            int objCount = sorted.Count<SqlSmoObject>();
            if (objCount == 0) return;

            SqlSmoObject[] smoReverseSorted = new SqlSmoObject[objCount];
            for (int index = 0; index < objCount; index++)
            {
                smoReverseSorted[index] = sorted[objCount - 1 - index];
            }

            if (sorted.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoReverseSorted);
            if (smoUDTs.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoUDTs);
            if (smoUDTTs.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoUDTTs);
            if (smoSchemas.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoSchemas);
            if (smoSynonyms.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoSynonyms);
            if (smoAssemblies.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoAssemblies);
            if (smoPartitionFunctions.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoPartitionFunctions);
            if (smoPartitionSchemas.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoPartitionSchemas);
            if (smoRoles.Count<SqlSmoObject>() > 0)
            {
                SqlSmoObject[] roles = new SqlSmoObject[1];
                foreach (SqlSmoObject role in smoRoles)
                {
                    roles[0] = role;
                    sdb.ScriptDrops(roles);
                }
            }
        }

        private void GenerateScriptFromSourceServer()
        {
            DateTime dtStart = DateTime.Now;
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(GenScriptAsyncUpdateStatusHandler);
            AsyncNotificationEventArgs args = new AsyncNotificationEventArgs(NotificationEventFunctionCode.GenerateScriptFromSQLServer, 0, "", CommonFunc.FormatString(Properties.Resources.MessageProcessStarted, dtStart.ToString(CultureInfo.CurrentUICulture), dtStart.ToUniversalTime().ToString(CultureInfo.CurrentUICulture)) + Environment.NewLine, Color.DarkBlue);
            StreamWriter swTSQL = null;
            SqlSmoObject[] smoTriggers = null;
            Object sender = System.Threading.Thread.CurrentThread;

            try
            {
                updateStatus(args);

                ServerConnection sc = new ServerConnection();

                sc.ServerInstance = _SourceServerInfo.ServerInstance;
                sc.SqlExecutionModes = SqlExecutionModes.ExecuteSql;
                sc.ConnectTimeout = (Int32)15;
                sc.DatabaseName = _SourceServerInfo.TargetDatabase;
                sc.LoginSecure = _SourceServerInfo.LoginSecure;
                if (!_SourceServerInfo.LoginSecure)
                {
                    sc.Login = _SourceServerInfo.Login;
                    sc.Password = _SourceServerInfo.Password;
                }
                sc.Connect();

                Server ss = new Server(sc);
                Database db = ss.Databases[_sourceDatabase.Name];

                ScriptDatabase sdb = new ScriptDatabase(_SourceServerInfo);
                sdb.Initialize(ss, db, updateStatus, _smoScriptOpts, swTSQL);

                args.DisplayText = "";
                args.StatusMsg = Properties.Resources.MessageSorting;
                args.PercentComplete = 1;
                updateStatus(args);

                // Tables, Views, Stored Procedures, and Triggers can all have dependencies.  GetSortedObjects returns
                // these objects in dependency order.

                SqlSmoObject[] sorted = GetSortedObjects(_sourceDatabase);
                if (_SourceServer.ConnectionContext.ServerVersion.Major < 10)
                {
                    smoTriggers = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeTriggers);
                }

                DateTime dtS = DateTime.Now;
                SqlSmoObject[] smoAssemblies = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeSQLAssemblies);
                SqlSmoObject[] smoSynonyms = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeSynonyms);
                SqlSmoObject[] smoRoles = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeRoles);
                SqlSmoObject[] smoSchemas = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeSchemas);
                SqlSmoObject[] smoSchemasCols = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeXMLSchemaCollections);
                SqlSmoObject[] smoUDTs = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeUDT);
                SqlSmoObject[] smoUDTTs = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeUDTT);
                SqlSmoObject[] smoPartitionFunctions = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypePartitionFunctions);
                SqlSmoObject[] smoPartitionSchemas = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypePartitionSchemes);
                Debug.WriteLine("Just getting GetSMOObjectListFromTreeViewNode: " + GetElapsedTime(dtS));

                if (Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDrop"), RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDropCreate"), RegexOptions.IgnoreCase))
                {
                    args.StatusMsg = Properties.Resources.MessageCreatingDropScripts;
                    args.PercentComplete = 2;
                    updateStatus(args);

                    ScriptDrops(sorted, smoAssemblies, smoSynonyms, smoRoles, smoSchemas, smoSchemasCols, smoUDTs, smoUDTTs, smoPartitionFunctions, smoPartitionSchemas, sdb);
                }

                if (Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSCreate"), RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDropCreate"), RegexOptions.IgnoreCase))
                {
                    SourceProcessor sp = new SourceProcessor();
                    sp.Initialize(sdb, _smoScriptOpts, updateStatus, args, ConfigurationManager.AppSettings["BCPFileDir"]);

                    // Assemblies, Synonyms, Roles, Schemas, XML Schema Collections and UDT have no dependencies.  Thus we process one at a time.

                    if (!Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase))
                    {
                        if (sp.Process(DatabaseObjectsTypes.Assemblies, smoAssemblies, 5)) return;
                        if (sp.Process(DatabaseObjectsTypes.Roles, smoRoles, 8)) return;
                        if (sp.Process(DatabaseObjectsTypes.Synonyms, smoSynonyms, 11)) return;
                        if (sp.Process(DatabaseObjectsTypes.Schemas, smoSchemas, 14)) return;
                        if (sp.Process(DatabaseObjectsTypes.XMLSchemaCollections, smoSchemasCols, 17)) return;
                        if (sp.Process(DatabaseObjectsTypes.UserDefinedDataTypes, smoUDTs, 20)) return;
                        if (sp.Process(DatabaseObjectsTypes.UserDefinedTableTypes, smoUDTTs, 23)) return;
                        if (sp.Process(DatabaseObjectsTypes.PartitionFunctions, smoPartitionFunctions, 26)) return;
                        if (sp.Process(DatabaseObjectsTypes.PartitionSchemes, smoPartitionSchemas, 30)) return;
                    }

                    if (sp.Process(sorted, 35)) return;

                    if (!Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase))
                    {
                        if (sp.Process(DatabaseObjectsTypes.Triggers, smoTriggers, 95)) return;
                    }
                }

                if (swTSQL != null)
                {
                    swTSQL.Flush();
                    swTSQL.Close();
                }

                DateTime dtEnd = DateTime.Now;
                string elapsedTime = GetElapsedTime(dtStart);

                args.StatusMsg = Properties.Resources.Done;
                args.DisplayColor = Color.DarkCyan;

                if (_smoScriptOpts.CheckCompatibility() == 1)
                {
                    args.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageFinishedNoAnalysis, dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), elapsedTime);
                }
                else
                {
                    args.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageFinishedWithAnalysis, dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), elapsedTime);
                }
            }
            catch (Exception ex)
            {
                args.DisplayColor = Color.Red;
                args.DisplayText = Environment.NewLine + CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailedAt, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine + Environment.NewLine + ex.ToString() + Environment.NewLine;
                args.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailed, ex.Message);
                args.DisableNext = true;
            }
            args.PercentComplete = 100;
            updateStatus(args);
        }

        private void ExecuteSQLonTarget()
        {
            AsyncQueueBCPJob queueBCPJob = new AsyncQueueBCPJob(AsyncQueueJobHandler);
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(TargetAsyncUpdateStatusHandler);
            TargetProcessor tp = new TargetProcessor();

            tp.ExecuteSQLonTarget(_TargetServerInfo, updateStatus, queueBCPJob, _sqlForAzure);
            tp.Close();
        }

        private void ParseTraceFile()
        {
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(StatusUpdateHandler);
            AsyncNotificationEventArgs e = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ParseFile, 0, "", "", Color.Black);
            ScriptDatabase sdb = new ScriptDatabase();
            TraceFile tf = null;
            int totalRecords = 0;

            sdb.Initialize(updateStatus, _smoScriptOpts, true);

            /****************************************************************/

            e.DisplayColor = Color.DarkBlue;
            e.DisplayText = Properties.Resources.MessageCalculatingSize;
            e.StatusMsg = Properties.Resources.MessageCalculatingNumberOrRecs + Environment.NewLine;
            e.PercentComplete = 0;
            updateStatus(e);

            try
            {
                // Part 1 -- Figure out number of records so that we can update the status bar in later processing

                using (tf = new TraceFile())
                {
                    int cnt = 0;
                    tf.InitializeAsReader(_FileToProcess);

                    while (tf.Read())
                    {
                        if (AsyncProcessingStatus.CancelProcessing) break;

                        ++totalRecords;
                        if (totalRecords % 10000 == 0)
                        {
                            ++cnt;
                            if (e.DisplayColor == Color.DarkBlue)
                            {
                                e.DisplayColor = Color.Brown;
                            }
                            else
                            {
                                e.DisplayColor = Color.DarkBlue;
                            }

                            if (cnt > 60)
                            {
                                cnt = 0;
                                e.DisplayText = "." + Environment.NewLine;
                            }
                            else
                            {
                                e.DisplayText = ".";
                            }

                            e.PercentComplete = 2;
                            updateStatus(e);
                        }
                    }
                    tf.Close();
                }
            }
            catch (Exception ex)
            {
                if (!AsyncProcessingStatus.CancelProcessing)
                {
                    e.DisplayColor = Color.Red;
                    e.DisplayText = Environment.NewLine + CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailedAt, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine + Environment.NewLine + ex.ToString() + Environment.NewLine;
                    e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailed, ex.Message);
                    e.PercentComplete = 100;
                    updateStatus(e);
                    return;
                }
            }

            if (!AsyncProcessingStatus.CancelProcessing)
            {
                int one = 1;
                e.DisplayColor = Color.DarkBlue;
                e.DisplayText = Environment.NewLine;
                e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingTraceFileStatus, one.ToString(CultureInfo.CurrentUICulture), totalRecords.ToString(CultureInfo.CurrentUICulture));
                e.PercentComplete = 3;
                updateStatus(e);

                string[] eventsToCheck = CommonFunc.GetAppSettingsStringValue("TraceEventsToCheck").Split('|');

                try
                {
                    // Part 2 -- Start over and analyze TSQL

                    using (tf = new TraceFile())
                    {
                        int loopCtr = 1;
                        tf.InitializeAsReader(_FileToProcess);

                        int ecOrdinal = tf.GetOrdinal("EventClass");
                        int tbOrdinal = tf.GetOrdinal("TextData");

                        while (tf.Read())
                        {
                            if (AsyncProcessingStatus.CancelProcessing) break;

                            string eventClass = tf.GetString(ecOrdinal);
                            if (eventClass != null && eventClass.Length > 0)
                            {
                                foreach (string etc in eventsToCheck)
                                {
                                    if (eventClass.Equals(etc, StringComparison.OrdinalIgnoreCase))
                                    {
                                        string sql = tf.GetString(tbOrdinal);
                                        if (sql != null && sql.Length > 0)
                                        {
                                            sdb.TSQLChecks(sql, null, loopCtr);
                                        }
                                        break;
                                    }
                                }
                            }

                            if (loopCtr++ % 20 == 0)
                            {

                                e.PercentComplete = (int)(((float)loopCtr / (float)totalRecords) * 100.0);
                                e.DisplayColor = Color.DarkBlue;
                                e.DisplayText = "";
                                e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingTraceFileStatus, loopCtr.ToString(CultureInfo.CurrentUICulture), totalRecords.ToString(CultureInfo.CurrentUICulture));
                                updateStatus(e);
                            }
                        }
                    }
                    tf.Close();
                }
                catch (Exception ex)
                {
                    if (!AsyncProcessingStatus.CancelProcessing)
                    {
                        e.DisplayColor = Color.Red;
                        e.DisplayText = Environment.NewLine + CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailedAt, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine + Environment.NewLine + ex.ToString() + Environment.NewLine;
                        e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailed, ex.ToString());
                        e.PercentComplete = 100;
                        updateStatus(e);
                        return;
                    }
                }
            }

            if (AsyncProcessingStatus.CancelProcessing)
            {
                e.StatusMsg = Properties.Resources.MessageCanceled;
                e.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageCanceledProcessing, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
            }
            else
            {
                e.DisplayColor = Color.DarkCyan;
                e.DisplayText = Properties.Resources.AnalysisComplete + Environment.NewLine;
                e.StatusMsg = Properties.Resources.Done;
            }
            e.PercentComplete = 100;
            updateStatus(e);
        }

        private void tbOutputFile_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text.Length > 0)
            {
                btnNext.Enabled = true;
            }
            else
            {
                btnNext.Enabled = false;
            }
        }
        private void tbOutputFolder_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text.Length > 0)
            {
                btnNext.Enabled = true;
            }
            else
            {
                btnNext.Enabled = false;
            }
        }
        private void ConnectToTargetServer()
        {
            if (_TargetServerInfo == null)
            {
                _TargetServerInfo = new TargetServerInfo();
                _TargetServerInfo.LoginSecure = CommonFunc.GetAppSettingsBoolValue("TargetConnectNTAuth");
                _TargetServerInfo.ServerType = CommonFunc.GetEnumServerType(((KeyValuePair<string, string>)cbTargetServerType.SelectedItem).Key); // CommonFunc.GetAppSettingsStringValue("TargetServerType"));
                _TargetServerInfo.ServerInstance = CommonFunc.GetAppSettingsStringValue("TargetServerName");
                _TargetServerInfo.Login = CommonFunc.GetAppSettingsStringValue("TargetUserName");
                _TargetServerInfo.Password = CommonFunc.GetAppSettingsStringValue("TargetPassword");
                _TargetServerInfo.RootDatabase = CommonFunc.GetAppSettingsStringValue("TargetDatabase");
            }

            SqlConnection connection = new SqlConnection();
            ServerConnect scForm = new ServerConnect(ref connection, ref _TargetServerInfo, true);
            DialogResult dr = scForm.ShowDialog(this);

            if (dr == DialogResult.OK)
            {
                if (_bckGndWkr.IsBusy)
                {
                    _bckGndWkr.CancelAsync();
                }
                Cursor orig = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                connection.Close();

                //ServerConnection connection = scForm.ServerConn;
                ConnectToServer(connection);
                Cursor.Current = orig;
            }
        }

        private void ConnectToServer(SqlConnection connection)
        {
            if (connection == null) return;

            DisplayDatabases(connection, lbTargetDatabases);

            if (_TargetServerInfo.RootDatabase.Length > 0 && !_TargetServerInfo.RootDatabase.Equals("master", StringComparison.OrdinalIgnoreCase))
            {
                btnCreateDatabase.Enabled = false;
                btnDeleteDatabase.Enabled = false;
                lbTargetDatabases.SelectedIndex = 0;
                btnNext.Enabled = true;
            }
            else
            {
                btnCreateDatabase.Enabled = true;
            }
        }

        private void btnConnectTargetServer_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            ConnectToTargetServer();
        }

        private void btnConnectToServer_Click(object sender, EventArgs e)
        {
            ServerConnection connection = new ServerConnection();

            btnNext.Enabled = false;
            //lbSelectDatabase.Text = Properties.Resources.SelectDatabase;

            if (_SourceServerInfo == null)
            {
                _SourceServerInfo = new TargetServerInfo();
                _SourceServerInfo.ServerInstance = ConfigurationManager.AppSettings["SourceServerName"];
                _SourceServerInfo.Login = ConfigurationManager.AppSettings["SourceUserName"];
                _SourceServerInfo.Password = ConfigurationManager.AppSettings["SourcePassword"];
                _SourceServerInfo.LoginSecure = CommonFunc.GetAppSettingsBoolValue("SourceConnectNTAuth");
                _SourceServerInfo.ServerType = CommonFunc.GetEnumServerType(CommonFunc.GetAppSettingsStringValue("SourceServerType"));
                _SourceServerInfo.RootDatabase = ConfigurationManager.AppSettings["SourceDatabase"];
            }

            ServerConnect scForm = new ServerConnect(ref connection, ref _SourceServerInfo);
            DialogResult dr = scForm.ShowDialog(this);

            if ((dr == DialogResult.OK) && (connection.SqlConnectionObject.State == ConnectionState.Open))
            {
                Cursor orig = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                _SourceServer = CommonFunc.GetSmoServer(connection);

                if (_SourceServer != null)
                {
                    _smoScriptOpts.SourceEngineType = _SourceServer.ServerType;
                    _SourceServerInfo.TargetDatabase = scForm.SpecifiedDatabase ? scForm.SpecifiedDatabaseName : "";
                    DisplayDatabases(ref _SourceServer, lbDatabases);
                    if (scForm.SpecifiedDatabase)
                    {
                        lbDatabases.SelectedIndex = 0;
                        btnNext.Enabled = true;
                    }
                    _SourceServer.ConnectionContext.Disconnect();
                }
                Cursor.Current = orig;
            }
        }

        private void btnFindFile_Click(object sender, EventArgs e)
        {
            if (rbTSQLFolder.Checked)
            {
                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

                folderBrowserDialog1.ShowNewFolderButton = false;
                folderBrowserDialog1.Description = Properties.Resources.MessageFolderToProcess;

                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    tbSourceFile.Text = folderBrowserDialog1.SelectedPath;
                    btnNext.Enabled = true;
                }
            }
            else
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.CheckPathExists = true;
                if (rbAnalyzeTraceFile.Checked)
                {
                    openFileDialog1.Filter = Properties.Resources.DialogFilterTrace;
                    openFileDialog1.Title = Properties.Resources.DialogTitleTrace;
                }
                else
                {
                    openFileDialog1.Filter = Properties.Resources.DialogFilterTSQL;
                    openFileDialog1.Title = Properties.Resources.DialogTitleTSQL;
                }

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    tbSourceFile.Text = openFileDialog1.FileName;
                    btnNext.Enabled = true;
                }
            }
        }

        private void lbDatabases_DoubleClick(object sender, EventArgs e)
        {
            if (lbDatabases.SelectedItem != null && ((DatabaseInfo)lbDatabases.SelectedItem).IsDbOwner)
            {
                _Reset = true;
                btnNext_Click(sender, e);
            }
        }

        private void btnCancelTargetProcessing_Click(object sender, EventArgs e)
        {
            AsyncProcessingStatus.CancelProcessing = true;
            CancelAsyncProcesses();
            btnBack.Enabled = true;
            btnCancelTargetProcessing.Enabled = false;
            progressBarTargetServer.Value = 0;
            lbStatus.Text = Properties.Resources.MessageCanceled;
        }

        private void btnCancelProcessing_Click(object sender, EventArgs e)
        {
            AsyncProcessingStatus.CancelProcessing = true;
            CancelAsyncProcesses();
            btnBack.Enabled = true;
            progressBarGenScript.Value = 0;
            btnCancelProcessing.Enabled = false;
            lbResultsSummaryStatus.Text = Properties.Resources.MessageCanceled;
        }

        private void rtbSQLScript_VScroll(object sender, EventArgs e)
        {
            cbResultsScroll.Focus();
        }

        private void rtbOverwriteFiles_VScroll(object sender, EventArgs e)
        {
            cbResultsScroll.Focus();
        }

        private void rtbAzureStatus_VScroll(object sender, EventArgs e)
        {
            cbAzureStatusScroll.Focus();
        }
        private void rtbResultsSummary_VScroll(object sender, EventArgs e)
        {
            cbResultsScroll.Focus();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = null;
            string title = "";

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.Title = title;
            saveFileDialog1.Filter = Properties.Resources.DialogFilterRTF;

            switch (_wizardIndex)
            {
                case WizardSteps.TargetResults:
                    rtb = rtbTargetResults;
                    title = Properties.Resources.SaveAzureResults;
                    break;

                case WizardSteps.ResultsSummary:
                    if (tabCtlResults.SelectedIndex == 0)
                    {
                        rtb = rtbResultsSummary;
                        title = Properties.Resources.SaveResults;
                    }
                    else
                    {
                        rtb = rtbSQLScript;
                        title = Properties.Resources.SaveSQLScript;
                        saveFileDialog1.Filter = Properties.Resources.DialogFilterSQLRTF;
                    }
                    break;
            }
            this.Cursor = Cursors.Default;

            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            string file = saveFileDialog1.FileName;
            try
            {
                StreamWriter sw = null;
                string ext = Path.GetExtension(file);
                if (ext.Equals(".rtf", StringComparison.CurrentCultureIgnoreCase))
                {
                    sw = new StreamWriter(file);
                    sw.Write(rtb.Rtf);
                    sw.Close();
                }
                else
                {
                    sw = new StreamWriter(file);
                    foreach (string str in rtb.Lines)
                    {
                        sw.WriteLine(str);
                    }
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
                Cursor.Current = Cursors.Default;
                return;
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show(Properties.Resources.Done);
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAnalyzeMigrateTSQLFile.Checked || rbAnalyzeTraceFile.Checked || rbRunTSQLFile.Checked || rbTSQLFolder.Checked)
            {
                if (tbSourceFile.Text.Length > 0)
                {
                    btnNext.Enabled = true;
                }
                else
                {
                    btnNext.Enabled = false;
                }

                if (rbTSQLFolder.Checked)
                {
                    lbDataToProcess.Text = Properties.Resources.MessageDirectoryToProcess;
                }
                else
                {
                    lbDataToProcess.Text = Properties.Resources.MessageFileToProcess;
                }
                panelFileToProcess.Visible = true;
            }
            else
            {
                btnNext.Enabled = true;
                panelFileToProcess.Visible = false;
                if (rbMaintenance.Checked) return;
            }

            if (rbRunTSQLFile.Checked)
            {
                _smoScriptOpts.CompatibilityChecks = _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsOverrideNot");
            }
            else
            {
                if (ConfigurationManager.AppSettings["CompatibilityChecks"].Equals("ScriptOptionsOverrideNot"))
                {
                    _smoScriptOpts.CompatibilityChecks = _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsUseDefault");
                }
                else
                {
                    _smoScriptOpts.CompatibilityChecks = _smoScriptOpts.GetAppConfigCompabilitySetting();
                }
            }

            _smoScriptOpts.Migrate = !rbAnalyzeTraceFile.Checked;
        }

        private void rbScriptAll_CheckedChanged(object sender, EventArgs e)
        {
            tvDatabaseObjects.Enabled = !rbScriptAll.Checked;
            if (rbScriptAll.Checked == true)
            {
                btnSelectAll_Click(sender, e);
                btnSelectAll.Enabled = false;
                btnClearAll.Enabled = false;
                btnNext.Enabled = true;
            }
        }

        private void rbSpecificObjects_CheckedChanged(object sender, EventArgs e)
        {
            tvDatabaseObjects.Enabled = rbSpecificObjects.Checked;
            if (rbSpecificObjects.Checked == true)
            {
                btnClearAll_Click(sender, e);
                btnSelectAll.Enabled = true;
                btnClearAll.Enabled = true;
                btnNext.Enabled = false;
            }
        }

        private int ChildNodesSelected(TreeNode node)
        {
            if (node.Nodes.Count == 0) return 0;

            int cnt = 0;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Checked) cnt++;
            }
            return cnt;
        }

        private void tvDatabaseObjects_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode tn = e.Node;

            if (tn == null) return;

            if (tn.ForeColor == Color.Red && tn.Checked)
            {
                tn.Checked = false;
                return;
            }

            if (_IgnorCheck) return;

            _IgnorCheck = true;


            if (tn.Parent != null)
            {
                int cnt = ChildNodesSelected(tn.Parent);
                if (cnt > 0)
                {
                    tn.Parent.Checked = true;
                }
                else
                {
                    tn.Parent.Checked = false;
                }
            }
            else if (tn.Nodes.Count > 0)
            {
                foreach (TreeNode child in tn.Nodes)
                {
                    if (child.ForeColor == Color.Red) continue;

                    child.Checked = tn.Checked;
                }
            }

            // Ok, we need to see if any nodes are checked to figure out if we need to enable the next button
            if (tn.Checked)
            {
                btnNext.Enabled = true;
            }
            else
            {
                btnNext.Enabled = false;
                foreach (TreeNode top in tvDatabaseObjects.Nodes)
                {
                    if (top.Checked)
                    {
                        btnNext.Enabled = true;
                        break;
                    }
                }
            }

            _IgnorCheck = false;
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            AdvancedSettings advanced = new AdvancedSettings(_smoScriptOpts);
            DialogResult dr = advanced.ShowDialog(this);

            if (dr == DialogResult.OK)
            {
                _smoScriptOpts = advanced.GetOptions;
            }
        }

        private void lbTargetDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTargetDatabases.SelectedIndex < 0)
            {
                btnNext.Enabled = false;
                btnDeleteDatabase.Enabled = false;
            }
            else if (_TargetServerInfo.RootDatabase.Length > 0 && !_TargetServerInfo.RootDatabase.Equals("master", StringComparison.OrdinalIgnoreCase)) // if this is specified, then user can't delete the database
            {
                if (!rbMaintenance.Checked) btnNext.Enabled = true;
                btnDeleteDatabase.Enabled = false;
            }
            else
            {
                if (!rbMaintenance.Checked) btnNext.Enabled = true;
                btnDeleteDatabase.Enabled = true;
            }
        }

        private void btnCreateDatabase_Click(object sender, EventArgs e)
        {
            CreateDatabase cdForm = new CreateDatabase(_TargetServerInfo);
            DialogResult dr = cdForm.ShowDialog(this);

            if (dr == DialogResult.OK)
            {
                lbTargetDatabases.Items.Add(cdForm.DatabaseName);
                lbTargetDatabases.SelectedIndex = lbTargetDatabases.Items.Count - 1;
                lbTargetDatabases.SelectedItem = cdForm.DatabaseName;
            }
            else if (dr == DialogResult.Retry)
            {
                MessageBox.Show(Properties.Resources.CreateDBInProcess);
                progressBarWaiting.Visible = true;
                _bckGndWkr.RunWorkerAsync();
            }
        }

        private void btnDeleteDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                if (lbTargetDatabases.SelectedItem != null)
                {
                    string database = lbTargetDatabases.SelectedItem.ToString();
                    if (MessageBox.Show(CommonFunc.FormatString(Properties.Resources.MessageAreYouSure, database), Properties.Resources.DeleteDatabase, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        using (SqlConnection sqlConnection = new SqlConnection(_TargetServerInfo.ConnectionStringRootDatabase))
                        {
                            sqlConnection.Open();

                            SqlCommand cmd = new SqlCommand("DROP DATABASE " + database, sqlConnection);
                            cmd.CommandType = CommandType.Text;

                            cmd.ExecuteNonQuery();
                            sqlConnection.Close();
                        }
                        lbTargetDatabases.Items.RemoveAt(lbTargetDatabases.SelectedIndex);
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == -2)
                {
                    MessageBox.Show(Properties.Resources.DeleteDBinProcess);
                    lbTargetDatabases.Items.RemoveAt(lbTargetDatabases.SelectedIndex);
                }
                else
                {
                    ErrorHelper.ShowException(this, ex);
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void lbTargetDatabases_DoubleClick(object sender, EventArgs e)
        {
            if (!rbMaintenance.Checked && lbTargetDatabases.SelectedItem != null)
            {
                _Reset = true;
                btnNext_Click(sender, e);
            }
        }

        private void DisplayChangedFilesLists()
        {
            //tabCtlOverwriteFile.SelectedIndex = 0;
            //rtbOverwriteSummaryStatus
            //System.Drawing.Size sz = new System.Drawing.Size(lbOverwriteSummaryStatus.Width, System.Int32.MaxValue);
            //sz = TextRenderer.MeasureText(String.Concat(CommonFunc.FormatString(Properties.Resources.rtbMessageOverwriteOriginalFiles, _AfterFilePath.FullName), _DirectoryToProcess.FullName, _BeforeFilePath.FullName), this.Font, sz, TextFormatFlags.WordBreak);
            //lbOverwriteSummaryStatus.Height = sz.Height * 2;

            rtbOverwriteFileMessage.Clear();
            rtbOverwriteSummaryStatus.Clear();

            AppendText(rtbOverwriteSummaryStatus, CommonFunc.FormatString(Properties.Resources.rtbMessageOverwriteOriginalFiles, _DirectoryToProcess) + Environment.NewLine, Color.Black);
            AppendText(rtbOverwriteSummaryStatus, CommonFunc.FormatString(Properties.Resources.rtbMessageOverwriteCopyOfOriginalFiles, _BeforeFilePath) + Environment.NewLine, Color.Black);
            AppendText(rtbOverwriteSummaryStatus, CommonFunc.FormatString(Properties.Resources.rtbMessageOverwriteImpactedFiles, _AfterFilePath), Color.Black);

            tabCtlOverwriteFile.Location = new Point(5, 80);
            int countOfFilesBeingProcessed = 0;
            if (_FilesModified != null)
            {
                countOfFilesBeingProcessed = _FilesModified.Count(t => t != null);
            }

            AppendText(rtbOverwriteFileMessage, CommonFunc.FormatString(Properties.Resources.rtbOverwriteFileMessageCountImpactedFiles, countOfFilesBeingProcessed) + Environment.NewLine + Environment.NewLine, Color.Black);
            if (countOfFilesBeingProcessed == 0)
            {
                btnOverwrite.Visible = false;
            }

            foreach (FileInfo f in _FilesModified)
            {
                if (f != null)
                {
                    string partialFilePath = f.FullName.Remove(0, _AfterFilePath.FullName.Length);
                    AppendText(rtbOverwriteFileMessage, String.Concat("...", partialFilePath), Color.DarkBlue);
                    AppendText(rtbOverwriteFileMessage, Environment.NewLine, Color.DarkBlue);
                    countOfFilesBeingProcessed++;
                }
            }
        }

        private void OverwriteSQLFiles()
        {
            rtbOverwriteFileMessage.Clear();
            rtbOverwriteSummaryStatus.Clear();

            int countFileModified = 0;
            int countFailedToProcessfile = 0;
            foreach (FileInfo f in _FilesModified)
            {
                if (f != null)
                {
                    try
                    {
                        string targetDirectoryPath = _DirectoryToProcess.FullName + f.DirectoryName.Remove(0, _AfterFilePath.FullName.Trim('\\').Length);
                        String copyFile = String.Concat(targetDirectoryPath, "\\", f.Name);
                        File.Copy(f.FullName, copyFile, true);
                        AppendText(rtbOverwriteFileMessage, CommonFunc.FormatString(Properties.Resources.MessageFileoverwriteSuccess, f.Name), Color.Black);
                        AppendText(rtbOverwriteFileMessage, Environment.NewLine, Color.Black);
                        countFileModified++;
                    }
                    catch (Exception ex)
                    {
                        AppendText(rtbOverwriteFileMessage, CommonFunc.FormatString(Properties.Resources.MessageFileoverwriteFailure, string.Concat(f.Name + " " + ex.Message)), Color.Red);
                        AppendText(rtbOverwriteFileMessage, Environment.NewLine, Color.Black);
                        //ErrorHelper.ShowException(this, ex);
                        countFailedToProcessfile++;
                    }
                }
            }
            AppendText(rtbOverwriteSummaryStatus, CommonFunc.FormatString(Properties.Resources.MessageOverwriteFilesSummary, countFileModified, _DirectoryToProcess), Color.Black);
            AppendText(rtbOverwriteSummaryStatus, Environment.NewLine, Color.Black);
            if (countFailedToProcessfile != 0)
            {
                AppendText(rtbOverwriteSummaryStatus, CommonFunc.FormatString(Properties.Resources.MessageOverwriteFilesSummary1, countFailedToProcessfile), Color.Red);
                AppendText(rtbOverwriteSummaryStatus, Environment.NewLine, Color.Black);
            }
            AppendText(rtbOverwriteSummaryStatus, CommonFunc.FormatString(Properties.Resources.MessageOverwriteFilesSummary2, _BeforeFilePath), Color.Black);
        }
    }
}
