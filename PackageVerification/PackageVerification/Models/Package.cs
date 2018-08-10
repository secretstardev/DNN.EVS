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
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using PackageVerification.AssemblyScanner;
using PackageVerification.AssemblyScanner.Models;

namespace PackageVerification.Models
{
    [Serializable]
    public class Package
    {
        private List<Manifest> _dnnManifests;
        private List<Assembly> _assemblies;
        public string Path { get; set; }
        public string SQLOutputPath { get; set; }
        public List<VerificationMessage> Messages { get; set; }
        public string ExtractedPath { get; set; }
        public bool HasErrors { get; set; }
        public bool HasWarnings { get; set; }
        public bool HasInfos { get; set; }
        public string CoreVersionDependency { get; set; }
        public string MinDotNetNukeVersion { get; set; }
        public string MinDotNetVersion { get; set; }
        
        [XmlIgnore]
        public List<Assembly> Assemblies
        {
            get
            {
                if (_assemblies == null)
                {
                    _assemblies = new List<Assembly>();

                    _assemblies.AddRange(ProcessAssembly.ProcessDirectory(ExtractedPath));
                    
                    var directories = Utilities.GetSubFolders(new List<string>(), ExtractedPath);

                    foreach (var directory in directories)
                    {
                        _assemblies.AddRange(ProcessAssembly.ProcessDirectory(directory));
                    }
                }

                return _assemblies;
            }
        }

        [XmlIgnore]
        public List<Manifest> DNNManifests
        {
            get
            {
                if(_dnnManifests == null)
                {
                    LoadManifests();
                }

                return _dnnManifests;
            }
        }

        public void CleanUp()
        {
            try
            {
                //System.IO.Directory.Delete(ExtractedPath,true);
            }
            catch (Exception)
            {

            }
        }
        
        private void LoadManifests()
        {
            var manifests = System.IO.Directory.GetFiles(ExtractedPath, "*.dnn");

            _dnnManifests = new List<Manifest>();

            foreach (var manifest in manifests)
            {
                var xml = new XmlDocument();
                xml.Load(manifest);

                if (manifest.EndsWith(".dnn"))
                {
                    _dnnManifests.Add(new Manifest(xml, manifest, ManifestExtensionTypes.dnn));
                }
                else if (manifest.EndsWith(".dnn5"))
                {
                    _dnnManifests.Add(new Manifest(xml, manifest, ManifestExtensionTypes.dnn5));
                }
                else if (manifest.EndsWith(".dnn6"))
                {
                    _dnnManifests.Add(new Manifest(xml, manifest, ManifestExtensionTypes.dnn6));
                }
            }
        }
    }
}