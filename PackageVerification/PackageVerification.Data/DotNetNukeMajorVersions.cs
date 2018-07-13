using System.Collections.Generic;

namespace PackageVerification.Data
{
    public class DotNetNukeMajorVersions
    {
        public static List<DotNetNukeVersionInfo> DotNetNukeVersionList()
        {
            var output = new List<DotNetNukeVersionInfo>();
            output.Add(new DotNetNukeVersionInfo("06.00.00", "07/20/11", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.01.00", "11/01/11", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.02.00", "05/29/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("07.00.00", "11/28/12", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.01.00", "07/09/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.02.00", "12/04/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.03.00", "06/11/14", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.04.00", "02/04/15", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "01/14/16", ReleaseType.Stable, "4.5", "2008", true));

            return output;
        }
    }
}