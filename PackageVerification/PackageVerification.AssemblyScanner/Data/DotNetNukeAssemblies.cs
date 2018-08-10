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
using PackageVerification.AssemblyScanner.Models;
using PackageVerification.Data;

namespace PackageVerification.AssemblyScanner.Data
{
    public class DotNetNukeAssemblies
    {
        public static List<DotNetNukeAssembliesInfo>  BuildDotNetNukeAssembliesCollection()
        {
            var output = new List<DotNetNukeAssembliesInfo>();

            var dotNetNukePackages = DotNetNukeVersions.DotNetNukeVersionList();

            foreach (var dotNetNukeVersionInfo in dotNetNukePackages)
            {
                output.Add(new DotNetNukeAssembliesInfo
                    {
                        DNNVersion = dotNetNukeVersionInfo,
                        AssemblyList = ProcessAssembly.ProcessDirectory(dotNetNukeVersionInfo.PackageFolderPath() + "/bin")
                    });
            }

            return output;
        }

        public static List<DotNetNukeAssembliesInfo> GetDotNetNukeAssembliesCollection(string assemblyInfoFile)
        {
            var serializer = new Serialize();

            var dnnAssembliesColl = serializer.DeSerializeObject<List<DotNetNukeAssembliesInfo>>(assemblyInfoFile);

            if(dnnAssembliesColl == null)
            {
                dnnAssembliesColl = BuildDotNetNukeAssembliesCollection();

                serializer.SerializeObject<List<DotNetNukeAssembliesInfo>>(dnnAssembliesColl, assemblyInfoFile);
            }

            return dnnAssembliesColl;
        }
    }

    [Serializable]
    public class DotNetNukeAssembliesInfo
    {
        public DotNetNukeVersionInfo DNNVersion { get; set; }
        public List<Assembly> AssemblyList { get; set; }

        public DotNetNukeAssembliesInfo()
        {
            DNNVersion = new DotNetNukeVersionInfo();
            AssemblyList = new List<Assembly>();
        }
    }
}
