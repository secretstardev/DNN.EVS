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
