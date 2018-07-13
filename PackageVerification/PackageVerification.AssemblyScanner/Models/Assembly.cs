using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using PackageVerification.Data;

namespace PackageVerification.AssemblyScanner.Models
{
    [Serializable]
    public class Assembly
    {
        private FrameworkInfo _framework;
        public string Name { get; set; }
        public string Version { get; set; }
        public string Culture { get; set; }
        public string PublicKeyToken { get; set; }

        [XmlIgnore]
        public List<Assembly> References { get; set; }
        
        public FrameworkInfo Framework
        {
            get
            {
                if(References.Count > 0)
                {
                    _framework = new FrameworkInfo();

                    foreach (var reference in References)
                    {
                        var assembliesCollection = Data.DotNetAssemblies.BuildDotNetAssembliesCollection();
                        foreach (var dotNetAssembliesInfo in assembliesCollection)
                        {
                            var assemblyList = dotNetAssembliesInfo.AssemblyList;

                            foreach (var assembly in assemblyList)
                            {
                                if (assembly.Name == reference.Name && assembly.Version == reference.Version && dotNetAssembliesInfo.DotNetVersion.SortOrder >= _framework.SortOrder)
                                {
                                    _framework = dotNetAssembliesInfo.DotNetVersion;
                                    goto Exit;
                                }
                            }
                        }

                    Exit:;
                    }
                }

                return _framework;
            }
            set { _framework = value; }
        }

        public Assembly()
        {
            Name = String.Empty;
            Framework = new FrameworkInfo();
            Version = String.Empty;
            Culture = String.Empty;
            PublicKeyToken = String.Empty;
            References = new List<Assembly>();
        }

        public Assembly(string name, string version)
        {
            Name = name;
            Framework = new FrameworkInfo();
            Version = version;
            Culture = String.Empty;
            PublicKeyToken = String.Empty;
            References = new List<Assembly>();
        }
    }
}
