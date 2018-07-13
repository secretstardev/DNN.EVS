using System;
using System.Collections.Generic;

namespace PackageVerification.Data
{
    public class DotNetVersions
    {
        public static List<FrameworkInfo> FrameworkList()
        {
            var output = new List<FrameworkInfo>();

            output.Add(new FrameworkInfo("1.0", "v1.0.3705", 1));
            output.Add(new FrameworkInfo("1.1", "v1.1.4322", 2));
            output.Add(new FrameworkInfo("2.0", "v2.0.50727", 3));
            output.Add(new FrameworkInfo("3.0", "v2.0.50727", 4));
            output.Add(new FrameworkInfo("3.5", "v2.0.50727", 5));
            output.Add(new FrameworkInfo("3.5 sp1", "v2.0.50727", 6));
            output.Add(new FrameworkInfo("4.0", "v4.0.30319", 7));
            output.Add(new FrameworkInfo("4.5", "v4.0.30319", 8));

            return output;
        }
    }

    public class FrameworkInfo
    {
        public string VersionName { get; set; }
        public string CLRVersion { get; set; }
        public int SortOrder { get; set; }

        public FrameworkInfo()
        {
            VersionName = "";
            CLRVersion = "";
            SortOrder = 0;
        }

        public FrameworkInfo(String versionName, String clrVersion, Int32 sortOrder)
        {
            VersionName = versionName;
            CLRVersion = clrVersion;
            SortOrder = sortOrder;
        }
    }
}
