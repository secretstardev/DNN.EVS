using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using SQLAzureMWUtils;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;

namespace SQLAzureMWBatchUpload
{
    partial class Program
    {
        private static string _FileToProcess = "";
        private static string _TargetDatabase = "";
        private static string _Collation = "";
        private static string _OutputResultsDirectory = "";
        private static string _OutputResultsFile = "";
        private static string _TargetServerName = "";
        private static string _TargetUserName = "";
        private static string _TargetPassword = "";
        private static string _TargetEdition = "";
        private static string _TargetDatabasePerforance = "";
        private static string _TargetDatabaseSize;
        private static bool _bTargetConnectNTAuth;
        private static bool _bDropExistingDatabase;

        static void Main(string[] args)
        {
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.ProgramVersion, assemName.Name, assemName.Version.ToString()));

            string message = "";
            if (!DependencyHelper.CheckDependencies(ref message))
            {
                Console.WriteLine(message);
                return;
            }

            string argValues = Environment.NewLine + Properties.Resources.ProgramArgs + Environment.NewLine;

            _TargetServerName = CommonFunc.GetAppSettingsStringValue("TargetServerName");
            _TargetUserName = CommonFunc.GetAppSettingsStringValue("TargetUserName");
            _Collation = CommonFunc.GetAppSettingsStringValue("DBCollation");
            _TargetPassword = CommonFunc.GetAppSettingsStringValue("TargetPassword");
            _FileToProcess = CommonFunc.GetAppSettingsStringValue("FileToProcess");
            _TargetDatabase = CommonFunc.GetAppSettingsStringValue("TargetDatabase");
            _TargetDatabaseSize = ConfigurationManager.AppSettings["TargetDatabaseSize"].ToUpper(CultureInfo.InvariantCulture);
            _TargetEdition = CommonFunc.GetAppSettingsStringValue("TargetDatabaseEdition").ToLower(CultureInfo.InvariantCulture);
            _TargetDatabasePerforance = CommonFunc.GetAppSettingsStringValue("TargetDatabasePerformanceLevel").ToUpper(CultureInfo.InvariantCulture);
            _OutputResultsFile = CommonFunc.GetAppSettingsStringValue("OutputResultsFile");
            _bTargetConnectNTAuth = CommonFunc.GetAppSettingsBoolValue("TargetConnectNTAuth");
            _bDropExistingDatabase = CommonFunc.GetAppSettingsBoolValue("DropOldDatabaseIfExists");

            for (int index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                    case "-S":
                    case "/S":
                        _TargetServerName = args[++index];
                        break;

                    case "-U":
                    case "/U":
                        _TargetUserName = args[++index];
                        break;

                    case "-P":
                    case "/P":
                        _TargetPassword = args[++index];
                        break;

                    case "-D":
                    case "/D":
                        _TargetDatabase = args[++index];
                        break;

                    case "-i":
                    case "/i":
                        _FileToProcess = args[++index];
                        break;

                    case "-c":
                    case "/c":
                        _Collation = args[++index];
                        break;

                    case "-o":
                    case "/o":
                        _OutputResultsFile = args[++index];
                        try
                        {
                            File.AppendAllText(_OutputResultsFile, "SQLAzureMWBatchUpload");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.ErrorOpeningFile, _OutputResultsFile, ex.Message));
                            return;
                        }
                        break;

                    case "-e":
                    case "/e":
                        _TargetEdition = args[++index].ToLower(CultureInfo.InvariantCulture);
                        if (!_TargetEdition.Equals("web") && !_TargetEdition.Equals("business") && !_TargetEdition.Equals("basic") && !_TargetEdition.Equals("standard") && !_TargetEdition.Equals("premium"))
                        {
                            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidDatabaseEdition, _TargetEdition));
                            return;
                        }
                        break;

                    case "-l":
                    case "/l":
                        _TargetDatabasePerforance = args[++index].ToUpper(CultureInfo.InvariantCulture);
                        break;

                    case "-s":
                    case "/s":
                        _TargetDatabaseSize = args[++index].ToUpper(CultureInfo.InvariantCulture);
                        break;

                    case "-T":
                    case "/T":
                        _bTargetConnectNTAuth = true;
                        break;

                    case "-d":
                    case "/d":
                        _bDropExistingDatabase = args[++index].Equals(Properties.Resources.True, StringComparison.CurrentCultureIgnoreCase);
                        break;

                    default:
                        Console.WriteLine(argValues);
                        return;
                }
            }
            Process();
        }

        protected static void TargetAsyncUpdateStatusHandler(AsyncNotificationEventArgs e)
        {
            int retry = 5;

            if (_OutputResultsFile.Length > 0)
            {
                while (true)
                {
                    try
                    {
                        File.AppendAllText(_OutputResultsFile, e.DisplayText);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (retry-- > 0)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        Console.WriteLine(ex.Message);
                        break;
                    }
                }
            }
            Console.Write(e.DisplayText);
        }

        public static void Process()
        {
            TargetProcessor tp = new TargetProcessor();
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(TargetAsyncUpdateStatusHandler);
            string sqlToExecute = CommonFunc.GetTextFromFile(_FileToProcess);

            _OutputResultsDirectory = _OutputResultsFile.Substring(0, _OutputResultsFile.LastIndexOf('\\') + 1);

            TargetServerInfo tsi = new TargetServerInfo();
            tsi.ServerInstance = _TargetServerName;
            tsi.TargetDatabase = _TargetDatabase;

            if (_bTargetConnectNTAuth == true)
            {
                // Use Windows authentication
                tsi.LoginSecure = true;
            }
            else
            {
                // Use SQL Server authentication
                tsi.LoginSecure = false;
                tsi.Login = _TargetUserName;
                tsi.Password = _TargetPassword;
            }

            tsi.Version = "";
            try
            {
                Retry.ExecuteRetryAction(() =>
                {
                    using (SqlConnection con = new SqlConnection(tsi.ConnectionStringRootDatabase))
                    {
                        ScalarResults sr = SqlHelper.ExecuteScalar(con, CommandType.Text, "SELECT @@VERSION");
                        string version = (string)sr.ExecuteScalarReturnValue;
                        if (version.IndexOf("Azure") > 0)
                        {
                            tsi.ServerType = ServerTypes.AzureSQLDB;
                        }
                        else
                        {
                            tsi.ServerType = ServerTypes.SQLServer;
                        }
                        Match result = Regex.Match(version, @"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+");
                        if (result.Success)
                        {
                            tsi.Version = result.Value;
                        }
                        else
                        {
                            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.ErrorGettingSQLVersion, version));
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            int dbMajor = Convert.ToInt32(tsi.Version.Substring(0, 2));

            //ok, we don't want to break the old version where they just passed in an int value without the "MB" or "GB".  So, we
            //can guess based on the edition what it should be.

            //web = 100 MB, 1 GB, 5 GB
            //business = 10 GB, 20 GB, 30 GB, 40 GB, 50 GB, 100 GB, or 150 GB
            //basic = 100 MB, 500 MB, 1 GB, 2 GB
            //standard = 100 MB, 500 MB, 1 GB, 2 GB, 5 GB, 10 GB, 20 GB, 30 GB, 40 GB, 50 GB, 100 GB, 150 GB, 200 GB, or 250 GB
            //premium = 100 MB, 500 MB, 1 GB, 2 GB, 5 GB, 10 GB, 20 GB, 30 GB, 40 GB, 50 GB, 100 GB, 150 GB, 200 GB, 250 GB, 300 GB, 400 GB, or 500 GB

            if (tsi.ServerType == ServerTypes.AzureSQLDB)
            {
                if (_TargetEdition == "web")
                {
                    if (dbMajor > 11)
                    {
                        Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidDatabaseEdition1V12, _TargetEdition));
                        return;
                    }

                    if (_TargetDatabaseSize[_TargetDatabaseSize.Length - 1] != 'B')
                    {
                        if (_TargetDatabaseSize == "1")
                        {
                            _TargetDatabaseSize = "1 GB";
                        }
                        else if (_TargetDatabaseSize == "5")
                        {
                            _TargetDatabaseSize = "5 GB";
                        }
                        else
                        {
                            _TargetDatabaseSize = "100 MB";
                        }
                    }
                }
                else if (_TargetEdition == "business")
                {
                    if (dbMajor > 11)
                    {
                        Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidDatabaseEdition1V12, _TargetEdition));
                        return;
                    }

                    if (_TargetDatabaseSize[_TargetDatabaseSize.Length - 1] != 'B')
                    {
                        _TargetDatabaseSize = _TargetDatabaseSize + " GB";
                    }
                }
                else if (_TargetEdition == "standard")
                {
                    if (dbMajor > 11)
                    {
                        if (!Regex.IsMatch(_TargetDatabasePerforance, "S[0-3]"))
                        {
                            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidPerformanceLevelV12, _TargetDatabasePerforance, _TargetEdition));
                            return;
                        }
                    }
                    else
                    {
                        if (!Regex.IsMatch(_TargetDatabasePerforance, "S[0-2]"))
                        {
                            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidPerformanceLevel, _TargetDatabasePerforance, _TargetEdition));
                            return;
                        }
                    }
                }
                else if (_TargetEdition == "premium")
                {
                    if (!Regex.IsMatch(_TargetDatabasePerforance, "P[1-3]"))
                    {
                        Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidPerformanceLevel, _TargetDatabasePerforance, _TargetEdition));
                        return;
                    }
                }
            }

            if (dbMajor == 12)
            {
                // Note with dbMajor 12 and above, disk size no longer matters
                _TargetDatabaseSize = "";
            }
            else
            {
                if (!ValidateDatabaseSize(_TargetEdition, _TargetDatabaseSize))
                {
                    Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidDatabaseSize, _TargetEdition, _TargetDatabaseSize));
                    return;
                }
            }

            //AsyncProcessingStatus.FinishedProcessingJobs = true;
            AsyncQueueBCPJob queueBCPJob = new AsyncQueueBCPJob(AsyncQueueJobHandler);
            DatabaseCreateStatus dcs = tp.CreateDatabase(tsi, _Collation, _TargetEdition, _TargetDatabaseSize, _TargetDatabasePerforance, _bDropExistingDatabase);
            if (dcs == DatabaseCreateStatus.Waiting)
            {
                Console.Write(Properties.Resources.WaitingForDBCreation);
                while (!tp.DoesDatabaseExist(tsi))
                {
                    Thread.Sleep(2000);
                    Console.Write(".");
                }
                Thread.Sleep(2000);
                Console.Write(Properties.Resources.DBCreated + Environment.NewLine);
            }
            else if (dcs == DatabaseCreateStatus.Failed)
            {
                Console.WriteLine(Properties.Resources.DBCreateFailed);
                return;
            }

            tp.ExecuteSQLonTarget(tsi, updateStatus, queueBCPJob, sqlToExecute);
        }

        private static bool SearchArray(string[] array, string value)
        {
            foreach (string size in array)
            {
                if (size.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool ValidateDatabaseSize(string edition, string dbSize)
        {
            string[] web = { "100 MB", "1 GB", "5 GB" };
            string[] business = { "10 GB", "20 GB", "30 GB", "40 GB", "50 GB", "100 GB", "150 GB" };
            string[] basic = { "100 MB", "500 MB", "1 GB", "2 GB" };
            string[] standard = { "100 MB", "500 MB", "1 GB", "2 GB", "5 GB", "10 GB", "20 GB", "30 GB", "40 GB", "50 GB", "100 GB", "150 GB", "200 GB", "250 GB" };
            string[] premium = { "100 MB", "500 MB", "1 GB", "2 GB", "5 GB", "10 GB", "20 GB", "30 GB", "40 GB", "50 GB", "100 GB", "150 GB", "200 GB", "250 GB", "300 GB", "400 GB", "500 GB" };
            bool valid = false;

            switch (edition)
            {
                case "web":
                    valid = SearchArray(web, dbSize);
                    break;

                case "business":
                    valid = SearchArray(business, dbSize);
                    break;

                case "basic":
                    valid = SearchArray(basic, dbSize);
                    break;

                case "standard":
                    valid = SearchArray(standard, dbSize);
                    break;

                case "premium":
                    valid = SearchArray(premium, dbSize);
                    break;

                default:
                    break;
            }
            return valid;
        }
    }
}