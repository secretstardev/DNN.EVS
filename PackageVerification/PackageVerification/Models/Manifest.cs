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

namespace PackageVerification.Models
{
    public class Manifest
    {
        public Manifest()
        {
            ManifestDoc = new XmlDocument();
            ManifestFileName = string.Empty;
            ManifestExtension = ManifestExtensionTypes.dnn;
            HasAzureCompatibleTag = false;
            TaggedAzureCompatible = false;
        }

        public Manifest(XmlDocument manifestDoc, string manifestFileName, ManifestExtensionTypes manifestExtension)
        {
            ManifestDoc = manifestDoc;
            ManifestFileName = manifestFileName;
            ManifestExtension = manifestExtension;
            Files.Add(manifestFileName.ToLower());
            HasAzureCompatibleTag = false;
            TaggedAzureCompatible = false;
        }
        
        private List<string> _files = new List<string>();
        private List<SqlScriptFile> _sqlScripts = new List<SqlScriptFile>();
        public XmlDocument ManifestDoc { get; set; }
        public string ManifestFileName { get; set; }

        public bool HasSqlScripts
        {
            get
            {
                if (_sqlScripts != null && _sqlScripts.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }

        public bool HasAzureCompatibleTag { get; set; }
        public bool TaggedAzureCompatible { get; set; }

        public ManifestExtensionTypes ManifestExtension { get; set; }
        public ManifestTypes ManifestType { get; set; }

        public double ManifestVersion()
        {
            var manifestVersion = 0.0;

            switch (ManifestExtension)
            {
                case ManifestExtensionTypes.dnn6:
                    manifestVersion = 6.0;
                    break;
                case ManifestExtensionTypes.dnn5:
                    manifestVersion = 5.0;
                    break;
                default:
                    {
                        var dotnetnukeNode = ManifestDoc.SelectSingleNode("dotnetnuke");
                        if (dotnetnukeNode != null)
                        {
                            if (dotnetnukeNode.Attributes != null && dotnetnukeNode.Attributes.Count > 0)
                            {
                                var versionAttr = dotnetnukeNode.Attributes["version"];
                                if (versionAttr != null && !string.IsNullOrEmpty(versionAttr.Value))
                                {
                                    double.TryParse(versionAttr.Value, out manifestVersion);
                                }
                            }
                        }
                    }
                    break;
            }

            return manifestVersion;
        }

        [XmlIgnore]
        public List<SqlScriptFile> SQLScripts
        {
            get
            {
                _sqlScripts.Sort(delegate(SqlScriptFile x, SqlScriptFile y)
                {
                    if (x == null)
                    {
                        if (y == null)
                        {
                            return 0;
                        }
                        return -1;
                    }

                    if (y == null)
                    {
                        return 0;
                    }
                    return x.Rank.CompareTo(y.Rank);
                });

                return _sqlScripts;
            }
            set
            {
                _sqlScripts = value;
            }
        }

        [XmlIgnore]
        public bool ContainsInstallScript
        {
            get { return SQLScripts.Any(sqlScriptFile => sqlScriptFile.IsInstall); }
        }

        [XmlIgnore]
        public int InstallScriptRank
        {
            get { return (from sqlScriptFile in SQLScripts where sqlScriptFile.IsInstall select sqlScriptFile.Rank).FirstOrDefault(); }
        }

        [XmlIgnore]
        public List<SqlScriptFile> InstallScripts
        {
            get { return SQLScripts.Where(sqlScriptFile => ContainsInstallScript == false || (sqlScriptFile.IsInstall || (sqlScriptFile.Rank > InstallScriptRank))).ToList(); }
        }

        [XmlIgnore]
        public List<string> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        [XmlIgnore]
        public IEnumerable<XmlNode> ComponentNodes
        {
            get{
                var xml = ManifestDoc;
                IEnumerable<XmlNode> output = null;

                if (xml != null)
                {
                    var packages = xml.SelectNodes("/dotnetnuke/packages/package");

                    if (packages != null)
                    {
                        foreach (XmlNode package in packages)
                        {
                            XmlNode componentsNode = package.SelectSingleNode("components");
                            if (componentsNode != null)
                            {
                                XmlNodeList componentNodes = componentsNode.SelectNodes("component");
                                if (componentNodes != null)
                                {
                                    if (output != null)
                                    {
                                        output = output.Concat(componentNodes.Cast<XmlNode>()).ToList();
                                    }
                                    else
                                    {
                                        output = componentNodes.Cast<XmlNode>();
                                    }
                                }
                            }
                        }
                    }
                }

                return output;
            }
        }
    }
    
    public enum ManifestExtensionTypes
    {
        dnn,
        dnn5,
        dnn6
    }

    public enum ManifestTypes
    {
        Package,
        Module
    }
}