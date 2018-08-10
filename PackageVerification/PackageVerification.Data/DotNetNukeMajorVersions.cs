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