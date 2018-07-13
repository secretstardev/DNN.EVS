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
