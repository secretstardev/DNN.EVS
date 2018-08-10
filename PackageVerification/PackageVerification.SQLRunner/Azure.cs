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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using PackageVerification.Data;

namespace PackageVerification.SQLRunner
{
    public class Azure
    {
        public static void ClearInstallAndRestoreDatabases()
        {
            try
            {
                //get connection strings
                var installConnectionString = System.Configuration.ConfigurationManager.AppSettings["AzureInstallConnectionString"];
                var restoreConnectionString = System.Configuration.ConfigurationManager.AppSettings["AzureRestoreConnectionString"];

                // Database deletion (using admin user and connecting to the master database)
                CleanupDatabase(installConnectionString);
                CleanupDatabase(restoreConnectionString);
            }
            catch (Exception ex)
            {
                //throw new ApplicationException(string.Format("An error occurred while cleaning up the temp and backup database: {0}", ex.Message), ex);
                Trace.TraceError("An error occurred while cleaning up the temp and backup database");
                Trace.TraceError(ex.Message);

                if (ex.InnerException != null)
                {
                    Trace.TraceError(ex.InnerException.Message);
                }
            }
        }

        public static void SetupBaseDNNDatabase(DotNetNukeVersionInfo version)
        {
            var connectionString = System.Configuration.ConfigurationManager.AppSettings["AzureInstallConnectionString"];

            LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/InstallCommon.sql", true);
            LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/InstallMembership.sql", true);
            LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/InstallProfile.sql", true);
            LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/InstallRoles.sql", true);
            LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/DotNetNuke.Schema.SqlDataProvider", true);
            LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/DotNetNuke.Data.SqlDataProvider", true);

            switch (version.Name)
            {
                case "06.00.00":
                    break;
                case "06.01.00":
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.00.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.00.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.01.00.SqlDataProvider", true);
                    break;
                case "06.02.00":
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.00.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.00.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.01.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.01.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.01.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.01.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.01.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.01.05.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/06.02.00.SqlDataProvider", true);
                    break;
                case "07.00.00":
                    break;
                case "07.01.00":
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider", true);
                    break;
                case "07.02.00":
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.00.SqlDataProvider", true);
                    break;
                case "07.03.00":
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.00.SqlDataProvider", true);
                    break;
                case "07.04.00":
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.04.00.SqlDataProvider", true);
                    break;
                case "08.00.00":
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.01.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.02.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.03.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.04.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.04.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/07.04.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.01.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.02.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.03.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.04.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.05.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.06.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.07.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.08.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.09.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.10.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.11.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.12.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.13.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.14.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.15.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.16.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.17.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.18.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.19.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.20.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.21.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.22.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.23.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.24.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.25.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.26.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.27.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.28.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.29.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.30.SqlDataProvider", true);
                    LoadAndRunSQLScriptViaSMO(connectionString, version.PackageFolderPathLive() + "/Providers/DataProviders/SqlDataProvider/08.00.00.31.SqlDataProvider", true);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Creates a bacpac file of the device's database and returns the local path of that filename
        /// </summary>
        public static void CreateDatabaseBackup()
        {
            var tempPathForbacpac = Path.Combine(Path.GetTempPath(), "azureBackup");

            Directory.CreateDirectory(tempPathForbacpac);

            var filepath = Path.Combine(tempPathForbacpac, "data.bac");

            //Trace.WriteLine("SQL Scanner - Azure database backup location: " + filepath, "Information");

            var i = 0;
            const int maxRetries = 3;
            while (true)
            {
                i++;
                try
                {
                    if (File.Exists(filepath)) File.Delete(filepath);

                    var dacSvc = new DacServices(System.Configuration.ConfigurationManager.AppSettings["AzureInstallConnectionString"]);
                    //dacSvc.Message += (sender, e) => Trace.WriteLine(e.Message);
                    dacSvc.ExportBacpac(filepath, System.Configuration.ConfigurationManager.AppSettings["TestInstallDBName"]);
                    break;
                }
                catch (Exception ex)
                {
                    if (i == maxRetries)
                        throw;
                    
                    Trace.WriteLine(string.Format("Exception occurred while creating the database backup for file {0} (retry {1} of {2}: {3}", filepath, i, maxRetries, ex.Message));

                    Trace.TraceError(ex.Message);

                    Trace.TraceError(ex.StackTrace);

                    if (ex.InnerException != null)
                    {
                        Trace.TraceError(ex.InnerException.Message);
                    }

                    Thread.Sleep(i * 1000);
                }
            }
        }
        
        /// <summary>
        /// Restores the backup of the database
        /// </summary>
        /// <remarks>The process of the bacpac restoration consists in drop the database from the server and restores the bacpac. This is because the bacpac restoration needs an
        /// empty database
        /// </remarks>
        public static void RestoreDatabaseBackup()
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.AppSettings["AzureRestoreConnectionString"];

                var tempPathForbacpac = Path.Combine(Path.GetTempPath(), "azureBackup");
                
                var filepath = Path.Combine(tempPathForbacpac, "data.bac");

                //Trace.WriteLine("SQL Scanner - Azure database backup location: " + filepath, "Information");

                if (File.Exists(filepath))
                {
                    // Database deletion (using admin user and connecting to the master database)
                    CleanupDatabase(connectionString);

                    // Uploading bacpac (using admin user and connecting to the user database)
                    var dacSvc = new DacServices(connectionString);
                    var package = BacPackage.Load(filepath);
                    dacSvc.ImportBacpac(package, System.Configuration.ConfigurationManager.AppSettings["TestRestoreDBName"]);
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("An error occurred while restoring the database backup: {0}", e.Message), e);
            }
        }
        
        public static void InstallScripts(string script)
        {
            var connectionString = System.Configuration.ConfigurationManager.AppSettings["AzureInstallConnectionString"];
            RunSQLScriptViaSMO(connectionString, script, true);
        }

        private static void LoadAndRunSQLScriptViaSMO(string connectionString, string scriptPath, bool replaceTokens)
        {
            var i = 0;
            const int maxRetries = 3;
            while (true)
            {
                i++;
                try
                {
                    RunSQLScriptViaSMO(connectionString, File.ReadAllText(scriptPath), true);
                    break;
                }
                catch (Exception ex)
                {
                    if (i == maxRetries)
                        throw;

                    Trace.WriteLine(string.Format("Exception occurred while running SQL Script {0} (retry {1} of {2}: {3}", scriptPath, i, maxRetries, ex.Message));

                    Trace.TraceError(ex.Message);

                    Trace.TraceError(ex.StackTrace);

                    if (ex.InnerException != null)
                    {
                        Trace.TraceError(ex.InnerException.Message);
                    }

                    Thread.Sleep(i * 1000);
                }
            }
        }

        private  static void RunSQLScriptViaSMO(string connectionString, string script, bool replaceTokens)
        {
            try
            {
                if (replaceTokens)
                script = ReplaceTokens(script);

                var conn = new SqlConnection(connectionString);
                var server = new Server(new ServerConnection(conn));

                server.ConnectionContext.ExecuteNonQuery(script);
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("An error occurred while running sql script via SMO: {0}", e.Message), e);
            }
        }

        internal static string ReplaceTokens(string input)
        {
            var output = input.Replace("{databaseOwner}", "[dbo].");
            output = output.Replace("{objectQualifier}", "");

            return output;
        }

        /// <summary>
        /// Cleans up the database
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        private static void CleanupDatabase(string connectionString)
        {
            var sqlCommand = string.Format(@"DECLARE @name varchar(256)
                                            DECLARE @type varchar(10)
                                            DECLARE @schema varchar(256)
                                            DECLARE @searchword varchar(250)
                                            DECLARE @command VARCHAR(2000)

                                            SET @searchword = '%'

                                            DECLARE @cursor CURSOR
                                            DECLARE @tname NVARCHAR(256)
                                            DECLARE @tschema NVARCHAR(256)
                                            DECLARE @cname NVARCHAR(256)
                                            DECLARE @cschema NVARCHAR(256)
                                            DECLARE @statement NVARCHAR(512)

                                            -- Remove all referential constraints before we start deleting objects.
                                            SET @cursor = CURSOR FAST_FORWARD FOR
	                                            SELECT
		                                            t2.TABLE_NAME,
		                                            t2.TABLE_SCHEMA,
		                                            t1.CONSTRAINT_NAME, 
		                                            t1.CONSTRAINT_SCHEMA
	                                            FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS t1
	                                            LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS t2 ON t1.CONSTRAINT_NAME = t2.CONSTRAINT_NAME
	
                                            OPEN @cursor FETCH NEXT FROM @cursor INTO @tname, @tschema, @cname, @cschema
                                            WHILE @@FETCH_STATUS = 0
                                            BEGIN
	                                            PRINT 'Dropping constraint [' + @cname + '] on table [' + @tschema + '].[' + @tname + ']...'
	                                            EXEC('ALTER TABLE [' + @tschema + '].[' + @tname + '] DROP [' + @cname + ']')

	                                            FETCH NEXT FROM @cursor INTO @tname, @tschema, @cname, @cschema
                                            END

                                            CLOSE @cursor
                                            DEALLOCATE @cursor

                                            -- Step 2: Drop objects
                                            DECLARE objects_cursor CURSOR FOR
                                            (
                                                SELECT O.name, O.type, S.NAME AS schema_name
                                                FROM sys.objects O
		                                            LEFT JOIN sys.schemas S ON O.schema_Id = S.schema_id
                                                WHERE O.name LIKE @searchword
		                                            AND (type IN ('U', 'P', 'FN', 'V', 'TF', 'IF'))
                                                    AND O.name NOT IN ('script_deployments', 'script_deployment_status', 'database_firewall_rules')
                                            )

                                            OPEN objects_cursor FETCH NEXT FROM objects_cursor INTO @name, @type, @schema
                                            WHILE @@FETCH_STATUS = 0
                                            BEGIN    
                                                SELECT @command =
                                                    CASE @type
                                                        WHEN 'P' THEN 'DROP PROCEDURE [' + @schema + '].[' + @name + ']'
                                                        WHEN 'U' THEN 'DROP TABLE [' + @schema + '].[' + @name + ']'
                                                        WHEN 'FN' THEN 'DROP FUNCTION [' + @schema + '].[' + @name + ']'
                                                        WHEN 'TF' THEN 'DROP FUNCTION [' + @schema + '].[' + @name + ']'
                                                        WHEN 'IF' THEN 'DROP FUNCTION [' + @schema + '].[' + @name + ']'
                                                        WHEN 'V' THEN 'DROP VIEW [' + @schema + '].[' + @name + ']'
                                                        ELSE ''        
                                                END
    
                                                IF (@command <> '')
                                                BEGIN
                                                    PRINT 'Dropping [' + @schema + '].[' + @name + ']...'
                                                    EXEC (@command)
                                                END
                                                ELSE
                                                BEGIN
                                                    PRINT 'WARNING: [' + @schema + '].[' + @name + '] will not be deleted'
                                                END 
    
                                                FETCH NEXT FROM objects_cursor INTO @name, @type, @schema    
                                            END
                                            CLOSE objects_cursor
                                            DEALLOCATE objects_cursor
                                            ");
            
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 60;
                        command.CommandText = sqlCommand;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("An error occurred while cleaning up the database: {0}", e.Message), e);
            }
        }
    }
}