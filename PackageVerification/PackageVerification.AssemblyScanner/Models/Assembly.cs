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
