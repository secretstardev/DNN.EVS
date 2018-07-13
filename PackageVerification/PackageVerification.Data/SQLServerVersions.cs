using System;
using System.Collections.Generic;

namespace PackageVerification.Data
{
    class SQLServerVersions
    {
        public static List<SQLServerVersionInfo> SQLServerVersionList()
        {
            var output = new List<SQLServerVersionInfo>();

            output.Add(new SQLServerVersionInfo("SQL Server 2000", "2000"));
            output.Add(new SQLServerVersionInfo("SQL Server 2005", "2005"));
            output.Add(new SQLServerVersionInfo("SQL Server 2008", "2008"));
            output.Add(new SQLServerVersionInfo("SQL Server 2008R2", "2008R2"));

            return output;
        }
    }

    public class SQLServerVersionInfo
    {
        public string VersionName { get; set; }
        public string ShortName { get; set; }

        public SQLServerVersionInfo()
        {
            VersionName = "";
            ShortName = "";
        }

        public SQLServerVersionInfo(String versionName, String shortName)
        {
            VersionName = versionName;
            ShortName = shortName;
        }
    }
}
