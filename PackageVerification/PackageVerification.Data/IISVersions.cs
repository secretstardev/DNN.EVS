using System;
using System.Collections.Generic;

namespace PackageVerification.Data
{
    class IISVersions
    {
        public static List<IISVersionInfo> IISVersionList()
        {
            var output = new List<IISVersionInfo>();

            output.Add(new IISVersionInfo("IIS 5.0"));
            output.Add(new IISVersionInfo("IIS 6.0"));
            output.Add(new IISVersionInfo("IIS 7.0"));
            output.Add(new IISVersionInfo("IIS 7.5"));

            return output;
        }
        
    }

    public class IISVersionInfo
    {
        public string VersionName { get; set; }

        public IISVersionInfo()
        {
            VersionName = "";
        }

        public IISVersionInfo(String versionName)
        {
            VersionName = versionName;
        }
    }
}
