using System;
using System.IO;
using PackageVerification.Data;

namespace PackageVerification.SQLRunner
{
    public class DNNBase
    {
        public static void InstallBaseDNNScripts(string databaseName, DotNetNukeVersionInfo version)
        {
            var basePath = version.PackageFolderPath();

            try
            {
                File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/InstallCommon.sql");
            }
            catch
            {
                basePath = version.PackageFolderPathLive();
            }

            Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/InstallCommon.sql"), true, true);
            Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/InstallMembership.sql"), true, true);
            Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/InstallProfile.sql"), true, true);
            Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/InstallRoles.sql"), true, true);
            Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/DotNetNuke.Schema.SqlDataProvider"), false, true);
            Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/DotNetNuke.Data.SqlDataProvider"), false, true);

            switch (version.Name)
            {
                case "06.00.00":
                    break;
                case "06.01.00":
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.00.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.00.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.01.00.SqlDataProvider"), false, true);
                    break;
                case "06.02.00":
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.00.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.00.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.01.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.01.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.01.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.01.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.01.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.01.05.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/06.02.00.SqlDataProvider"), false, true);
                    break;
                case "07.00.00":
                    break;
                case "07.01.00":
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider"), false, true);
                    break;
                case "07.02.00":
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.00.SqlDataProvider"), false, true);
                    break;
                case "07.03.00":
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.00.SqlDataProvider"), false, true);
                    break;
                case "07.04.00":
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.04.00.SqlDataProvider"), false, true);
                    break;
                case "08.00.00":
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.05.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.00.06.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.01.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.02.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.03.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.04.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.04.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/07.04.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.01.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.02.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.03.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.04.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.05.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.06.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.07.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.08.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.09.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.10.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.11.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.12.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.13.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.14.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.15.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.16.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.17.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.18.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.19.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.20.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.21.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.22.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.23.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.24.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.25.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.26.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.27.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.28.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.29.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.30.SqlDataProvider"), false, true);
                    Common.RunSQLScriptViaSMO(databaseName, File.ReadAllText(basePath + "/Providers/DataProviders/SqlDataProvider/08.00.00.31.SqlDataProvider"), false, true);
                    break;
                default:
                    break;
            }
        }
    }
}
