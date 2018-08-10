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

#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

// SMO namespaces
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer;
using System.Text.RegularExpressions;
using SQLAzureMWUtils;
using System.Diagnostics;
using System.IO;
using System.Threading;

#endregion

namespace SQLAzureMWUtils
{
    public partial class ServerConnect : Form
    {
        private const string HistoryFileName = "SQLAzureMW.dat";
        private List<TargetServerInfo> _servers;
        private bool _bSkip = false;
        private bool _ClickedProcessed = false;


        private TargetServerInfo _serverInfo = null;
        public SqlConnection SqlConn;
        public ServerConnection ServerConn;

        private Boolean _Warned;
        private Boolean _ErrorFlag;
        private Boolean _TargetServer;
        private string _OldDatabase;
        public Boolean SpecifiedDatabase { get { return rbSpecifiedDB.Checked; } }
        public Boolean LoginSecure { get { return WindowsAuthenticationRadioButton.Checked; } }
        public string SpecifiedDatabaseName { get { return tbDatabase.Text; } }
        public string ServerInstance { get { return ServerNamesComboBox.Text; } }
        public string UserName { get { return UserNameTextBox.Text; } }
        public string Password { get { return PasswordTextBox.Text; } }

        public ServerConnect(ref SqlConnection connection, ref TargetServerInfo serverInfo, bool target = false)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            SqlConn = connection;
            Initialize(ref serverInfo, target);
        }

        public ServerConnect(ref ServerConnection connection, ref TargetServerInfo serverInfo, bool target = false)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            ServerConn = connection;
            Initialize(ref serverInfo, target);
        }

        public void Initialize(ref TargetServerInfo serverInfo, bool target)
        {
            _TargetServer = target;
            Initialize(ref serverInfo);
        }

        private void ServerConnect_Load(object sender, EventArgs e)
        {
            ProcessWindowsAuthentication();
        }

        private void WindowsAuthenticationRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ProcessWindowsAuthentication();
        }

        private void Initialize(ref TargetServerInfo serverInfo)
        {
            _serverInfo = serverInfo;

            if (File.Exists(HistoryFileName))
            {
                try
                {
                    _servers = CommonFunc.DecompressObjectFromFile<List<TargetServerInfo>>(HistoryFileName);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.ToString());
                    _servers = new List<TargetServerInfo>();
                }
            }
            else
            {
                _servers = new List<TargetServerInfo>();
            }
            _bSkip = true;
            ServerNamesComboBox.DataSource = _servers;
            ServerNamesComboBox.DisplayMember = "ServerInstance";
            _bSkip = false;

            if (_servers.Count > 0)
            {
                foreach (TargetServerInfo info in _servers)
                {
                    if (info.ServerInstance.Equals(serverInfo.ServerInstance, StringComparison.OrdinalIgnoreCase) &&
                        info.ServerType == serverInfo.ServerType)
                    {
                        _ClickedProcessed = false;
                        ServerNamesComboBox.SelectedItem = info;
                        if (!_ClickedProcessed)
                        {
                            ServerNamesComboBox_SelectedIndexChanged(null, null);
                        }
                        return;
                    }
                }
            }

            if (serverInfo.ServerInstance.Length == 0 && _servers.Count > 0)
            {
                _ClickedProcessed = false;
                ServerNamesComboBox.SelectedIndex = 0;
                if (!_ClickedProcessed)
                {
                    ServerNamesComboBox_SelectedIndexChanged(null, null);
                }
                return;
            }
            UpdateForm(serverInfo);
        }

        private void CancelCommandButton_Click(object sender, EventArgs e)
        {
            ServerConn = null;
            this.Close();
        }

        private void ConnectServerConnection(TargetServerInfo serverInfo)
        {
            ServerConn.ServerInstance = serverInfo.ServerInstance;
            ServerConn.LoginSecure = serverInfo.LoginSecure;
            if (serverInfo.LoginSecure == false)
            {
                ServerConn.Login = serverInfo.Login;
                ServerConn.Password = serverInfo.Password;
            }
            ServerConn.DatabaseName = serverInfo.RootDatabase;
            ServerConn.SqlExecutionModes = SqlExecutionModes.ExecuteSql;
            ServerConn.Connect();
            serverInfo.Version = ServerConn.ServerVersion.ToString();
            if (ServerConn.DatabaseEngineType == DatabaseEngineType.SqlAzureDatabase)
            {
                serverInfo.ServerType = ServerTypes.AzureSQLDB;
            }
            else
            {
                serverInfo.ServerType = ServerTypes.SQLServer;
            }
        }

        private void ConnectSqlConnection(TargetServerInfo serverInfo)
        {
            // Go ahead and connect
            SqlConn.ConnectionString = serverInfo.ConnectionStringRootDatabase;
            Retry.ExecuteRetryAction(() =>
            {
                SqlConn.Open();
                ScalarResults sr = SqlHelper.ExecuteScalar(SqlConn, CommandType.Text, "SELECT @@VERSION");
                string version = (string)sr.ExecuteScalarReturnValue;
                if (version.IndexOf("Azure") > 0)
                {
                    serverInfo.ServerType = ServerTypes.AzureSQLDB;
                }
                else
                {
                    serverInfo.ServerType = ServerTypes.SQLServer;
                }
                serverInfo.Version = SqlConn.ServerVersion;
            });
        }
        
        private void ConnectCommandButton_Click(object sender, EventArgs e)
        {
            Cursor csr = null;

            try
            {
                csr = this.Cursor;   // Save the old cursor
                this.Cursor = Cursors.WaitCursor;   // Display the waiting cursor

                TargetServerInfo serverInfo = new TargetServerInfo();

                _ErrorFlag = false; // Assume no error

                if (rbSpecifiedDB.Checked && tbDatabase.Text.Length == 0)
                {
                    MessageBox.Show(Properties.Resources.MessageSpecifyDatabase);
                    tbDatabase.Focus();
                    return;
                }

                serverInfo.ServerInstance = ServerNamesComboBox.Text;
                serverInfo.RootDatabase = rbSpecifiedDB.Checked ? tbDatabase.Text : "master";
                serverInfo.LoginSecure = WindowsAuthenticationRadioButton.Checked;
                serverInfo.Save = rbSave.Checked;

                if (WindowsAuthenticationRadioButton.Checked == false)
                {
                    // Use SQL Server authentication
                    if (UserNameTextBox.Text.Length == 0)
                    {
                        MessageBox.Show(Properties.Resources.MessageUserName);
                        UserNameTextBox.Focus();
                        return;
                    }

                    serverInfo.Password = PasswordTextBox.Text;

                    if (Regex.IsMatch(ServerNamesComboBox.Text, Properties.Resources.SQLAzureCloudName, RegexOptions.IgnoreCase))
                    {
                        string svrName = ServerNamesComboBox.Text.Substring(0, ServerNamesComboBox.Text.IndexOf('.'));
                        string svrExt = "@" + svrName;
                        string usrSvrName = "";
                        int    idx = UserNameTextBox.Text.IndexOf('@');

                        if (idx > 0)
                        {
                            usrSvrName = UserNameTextBox.Text.Substring(idx);
                        }

                        if (!usrSvrName.Equals(svrExt))
                        {
                            if (idx < 1)
                            {
                                // Ok, the user forgot to put "@server" at the end of the user name.  See if they want
                                // us to add it for them.

                                DialogResult dr = MessageBox.Show(CommonFunc.FormatString(Properties.Resources.AzureUserName, svrExt), Properties.Resources.AzureUserNameWarning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                                if (dr == DialogResult.Yes)
                                {
                                    UserNameTextBox.Text = UserNameTextBox.Text + svrExt;
                                }
                            }
                            else
                            {
                                // Ok, to get there, the user added @server to the end of the username, but the @server does not match @server in Server name.
                                // Check to see if the user wants us to fix for them.


                                DialogResult dr = MessageBox.Show(CommonFunc.FormatString(Properties.Resources.ServerNamesDoNotMatch, usrSvrName, svrExt), Properties.Resources.AzureUserNameWarning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                                if (dr == DialogResult.Yes)
                                {
                                    UserNameTextBox.Text = UserNameTextBox.Text.Substring(0, idx) + svrExt;
                                }
                            }
                        }
                    }
                    serverInfo.Login = UserNameTextBox.Text;
                }

                // Go ahead and connect
                if (SqlConn != null)
                {
                    ConnectSqlConnection(serverInfo);
                }
                else if (ServerConn != null)
                {
                    ConnectServerConnection(serverInfo);
                }
                else
                {
                    ServerConn = new ServerConnection();
                    ConnectServerConnection(serverInfo);
                }

                bool isNew = true;
                foreach(TargetServerInfo svrInfo in _servers)
                {
                    if (svrInfo.ServerInstance.Equals(serverInfo.ServerInstance, StringComparison.OrdinalIgnoreCase) &&
                        svrInfo.ServerType == serverInfo.ServerType)
                    {
                        isNew = false;

                        svrInfo.Version = serverInfo.Version;
                        break;
                    }
                }

                if (isNew) _servers.Add(serverInfo);

                List<TargetServerInfo> saveList = new List<TargetServerInfo>();
                foreach (TargetServerInfo svrInfo in _servers)
                {
                    if (svrInfo.Save)
                    {
                        saveList.Add(svrInfo);
                    }
                }

                if (saveList.Count == 0)
                {
                    if (File.Exists(HistoryFileName))
                    {
                        File.Delete(HistoryFileName);
                    }
                }
                else
                {
                    CommonFunc.CompressObjectToFile(saveList, HistoryFileName);
                }
                _serverInfo.ServerInstance = serverInfo.ServerInstance;
                _serverInfo.ServerType = serverInfo.ServerType;
                _serverInfo.TargetDatabase = serverInfo.TargetDatabase;
                _serverInfo.Login = serverInfo.Login;
                _serverInfo.LoginSecure = serverInfo.LoginSecure;
                _serverInfo.Save = serverInfo.Save;
                _serverInfo.Password = serverInfo.Password;
                _serverInfo.RootDatabase = serverInfo.RootDatabase;
                _serverInfo.Version = serverInfo.Version;
                this.DialogResult = DialogResult.OK;
            }
            catch (ConnectionFailureException ex)
            {
                string message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(message, Properties.Resources.Error2, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ErrorFlag = true;
            }
            catch (SmoException ex)
            {
                string message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(message, Properties.Resources.Error2, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ErrorFlag = true;
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(message, Properties.Resources.Error2, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ErrorFlag = true;
            }
            finally
            {
                this.Cursor = csr;  // Restore the original cursor
            }
        }

        private void ServerConnect_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_ErrorFlag == true)
            {
                e.Cancel = true;
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }

            // Reset error condition
            _ErrorFlag = false;
        }

        private void ProcessWindowsAuthentication()
        {
            if (WindowsAuthenticationRadioButton.Checked == true)
            {
                UserNameTextBox.Enabled = false;
                PasswordTextBox.Enabled = false;
            }
            else
            {
                UserNameTextBox.Enabled = true;
                PasswordTextBox.Enabled = true;
            }
        }

        private void rbMasterDB_CheckedChanged(object sender, EventArgs e)
        {
            SpecifyDatabase();
        }

        private void SpecifyDatabase()
        {
            if (rbMasterDB.Checked == true)
            {
                tbDatabase.Enabled = false;
                _OldDatabase = tbDatabase.Text;
                tbDatabase.Text = "";
            }
            else
            {
                if (Regex.IsMatch(ServerNamesComboBox.Text, @"\.net", RegexOptions.IgnoreCase) && !_Warned && !_TargetServer)
                {
                    _Warned = true;
                    string warning = Properties.Resources.DBConnect + Environment.NewLine + Environment.NewLine + Properties.Resources.PerformanceWarningAreYouSure;
                    DialogResult dr = MessageBox.Show(warning, Properties.Resources.PerformanceWarning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.No)
                    {
                        rbMasterDB.Checked = true;
                        return;
                    }
                }
                tbDatabase.Enabled = true;
                tbDatabase.Text = _OldDatabase;
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (_TargetServer)
            {
                MessageBox.Show(Properties.Resources.DBConnect + Environment.NewLine + Environment.NewLine + Properties.Resources.DBConnectWarningTarget, Properties.Resources.MessageTitleConnectTarget, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(Properties.Resources.DBConnect, Properties.Resources.MessageTitleConnectSource, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateForm(TargetServerInfo serverInfo)
        {
            if (serverInfo == null)
            {
                UserNameTextBox.Text = "";
                PasswordTextBox.Text = "";
                tbDatabase.Text = "";
                WindowsAuthenticationRadioButton.Checked = false;
                gboxSaveServerInfo.Text = "";
            }
            else
            {
                _bSkip = true;
                ServerNamesComboBox.SelectedIndex = -1;
                ServerNamesComboBox.Text = serverInfo.ServerInstance == null ? "" : serverInfo.ServerInstance;
                _bSkip = false;

                UserNameTextBox.Text = serverInfo.Login == null ? "" : serverInfo.Login;
                PasswordTextBox.Text = serverInfo.Password == null ? "" : serverInfo.Password;
                tbDatabase.Text = _OldDatabase = serverInfo.RootDatabase == null ? "" : serverInfo.RootDatabase;

                gboxSaveServerInfo.Text = ServerNamesComboBox.Text;
                if (serverInfo.Save) rbSave.Checked = true;
                else rbForget.Checked = true;
            }

            if (serverInfo.LoginSecure)
            {
                WindowsAuthenticationRadioButton.Checked = true;
            }
            else
            {
                rbSpecifyUserPassword.Checked = true;
            }
        }

        private void ServerNamesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bSkip) return;

            _ClickedProcessed = true;
            UpdateForm((TargetServerInfo)ServerNamesComboBox.SelectedItem);
        }

        private void ServerNamesComboBox_Leave(object sender, EventArgs e)
        {
            gboxSaveServerInfo.Text = ServerNamesComboBox.Text;
        }
    }
}