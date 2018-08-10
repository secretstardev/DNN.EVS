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
