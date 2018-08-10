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
using System.Diagnostics;
using System.IO;
using System.Xml;
using PackageVerification.AssemblyScanner;

namespace PackageVerification.Rules.Manifest
{
    public class PackageNodeAndChildren : ManifestRulesBase, Models.IManifestRule
    {
        public List<Models.VerificationMessage> ApplyRule(Models.Package package, Models.Manifest manifest)
        {
            var r = new List<Models.VerificationMessage>();
            try
            {
                var xml = manifest.ManifestDoc;

                if (xml != null)
                {
                    var packages = xml.SelectSingleNode("/dotnetnuke/packages");

                    if (packages == null)
                    {
                        r.Add(new Models.VerificationMessage { Message = "Package manifest must contain a packages node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("f6bf2243-6c28-49d0-ace7-c140c359b1b9"), Rule = GetType().ToString() });
                    }
                    else 
                    {
                        if (!packages.HasChildNodes)
                        {
                            r.Add(new Models.VerificationMessage { Message = "Package manifest must contain a packages node with child nodes.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("a173a46d-ba69-48a6-8405-e1cabb894907"), Rule = GetType().ToString() });
                        }
                        
                        foreach (XmlNode packageNode in packages.ChildNodes)
                        {
                            if (packageNode.Attributes != null && packageNode.Attributes.Count > 0)
                            {
                                var pName = packageNode.Attributes["name"];
                                if (pName == null || string.IsNullOrEmpty(pName.Value))
                                {
                                    r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest must have a name.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("402b49aa-3e5b-4e92-92da-4b899a5687fb"), Rule = GetType().ToString() });
                                }
                                else
                                {
                                    r.Add(new Models.VerificationMessage { Message = pName.Value, MessageType = Models.MessageTypes.Info, MessageId = new Guid("d40fe32f-ea79-49c3-802c-f7264fb69a2b"), Rule = GetType().ToString() });
                                }
                                var pType = packageNode.Attributes["type"];
                                if (pType == null || string.IsNullOrEmpty(pType.Value))
                                {
                                    r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest must have a type.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("5c6ec9ea-498c-436a-892b-a3f7209b8f51"), Rule = GetType().ToString() });
                                }
                                else
                                {
                                    r.Add(new Models.VerificationMessage { Message = pType.Value, MessageType = Models.MessageTypes.Info, MessageId = new Guid("feaf4309-3ef3-4278-b610-b1f78fe3c6a9"), Rule = GetType().ToString() });
                                }
                                var pVersion = packageNode.Attributes["version"];
                                if (pVersion == null || string.IsNullOrEmpty(pVersion.Value))
                                {
                                    r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest must have a version.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("d67ea510-1dc0-468f-85b6-3f75212814b5"), Rule = GetType().ToString() });
                                }
                                else
                                {
                                    r.Add(new Models.VerificationMessage { Message = pVersion.Value, MessageType = Models.MessageTypes.Info, MessageId = new Guid("07e29bf3-5816-4e1c-a9e8-7a05ee02a0f0"), Rule = GetType().ToString() });
                                }
                            }
                            
                            var friendlyNameNode = packageNode.SelectSingleNode("friendlyName");
                            if (friendlyNameNode == null || string.IsNullOrEmpty(friendlyNameNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest must have a friendly name.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("d6d6dcaf-a5a2-486a-bb30-983b39528ed0"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = friendlyNameNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("671b6736-b686-485b-a269-7be54180517e"), Rule = GetType().ToString() });
                            }
                            
                            var descriptionNode = packageNode.SelectSingleNode("description");
                            if (descriptionNode == null || string.IsNullOrEmpty(descriptionNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest must have a description.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("efce2ed8-9748-493b-839d-7c6e370b88dc"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = descriptionNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("93959db4-b7b0-4917-bb76-465173b9dd2b"), Rule = GetType().ToString() });
                            }
                            
                            var ownerNode = packageNode.SelectSingleNode("owner");
                            if (ownerNode == null || string.IsNullOrEmpty(ownerNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest must have an owner.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("312001e5-166c-46f7-9f40-4641823d3c8c"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = ownerNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("9ba9bb5e-9d4c-4b7a-8bcb-166dd468ed2d"), Rule = GetType().ToString() });
                            }
                            
                            if (manifest.ManifestVersion() >= 5.0)
                            {
                                var licenseNode = packageNode.SelectSingleNode("license");
                                if (licenseNode != null)
                                {
                                    if (licenseNode.Attributes != null && licenseNode.Attributes.Count > 0)
                                    {
                                        var licenseSrc = licenseNode.Attributes["src"];
                                        if (licenseSrc != null && !string.IsNullOrEmpty(licenseSrc.Value))
                                        {
                                            var path = package.ExtractedPath;
                                            var fullFilePath = Path.Combine(path, licenseSrc.Value);

                                            manifest.Files.Add(fullFilePath.ToLower());
                                        }
                                        else
                                        {
                                            r.Add(new Models.VerificationMessage { Message = "The license node src attribute needs to have a value specified", MessageType = Models.MessageTypes.Error, MessageId = new Guid("2376cd6a-ace0-4210-b39f-0b2c20c1506a"), Rule = GetType().ToString() });
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(licenseNode.InnerText))
                                        {
                                            r.Add(new Models.VerificationMessage { Message = "The license node needs to have a src attribute or license should be specified in the license node inner text.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("d9099897-f8ec-4af3-a922-859c56a9352e"), Rule = GetType().ToString() });
                                        }
                                        
                                    }
                                }
                                else
                                {
                                    r.Add(new Models.VerificationMessage { Message = "The package should have a license node", MessageType = Models.MessageTypes.Error, MessageId = new Guid("f9255105-a561-4e43-ac4b-af177fb7da10"), Rule = GetType().ToString() });
                                }

                                var releaseNotesNode = packageNode.SelectSingleNode("releaseNotes");
                                if (releaseNotesNode != null)
                                {
                                    if (releaseNotesNode.Attributes != null && releaseNotesNode.Attributes.Count > 0)
                                    {
                                        var releaseNotesSrc = releaseNotesNode.Attributes["src"];
                                        if (releaseNotesSrc != null && !string.IsNullOrEmpty(releaseNotesSrc.Value))
                                        {
                                            var path = package.ExtractedPath;
                                            var fullFilePath = Path.Combine(path, releaseNotesSrc.Value);

                                            manifest.Files.Add(fullFilePath.ToLower());
                                        }
                                        else
                                        {
                                            r.Add(new Models.VerificationMessage { Message = "The releaseNotes node src attribute needs to have a value specified", MessageType = Models.MessageTypes.Error, MessageId = new Guid("e9ac7c38-808e-4300-b749-3c2ba453eb0a"), Rule = GetType().ToString() });
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(releaseNotesNode.InnerText))
                                        {
                                            r.Add(new Models.VerificationMessage { Message = "The releaseNotes node needs to have a src attribute or license should be specified in the license node inner text.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("c50278f4-6461-4fe8-873c-b8ec2587135d"), Rule = GetType().ToString() });
                                        }
                                    }
                                }
                                else
                                {
                                    r.Add(new Models.VerificationMessage { Message = "The package should have a releaseNotes node", MessageType = Models.MessageTypes.Error, MessageId = new Guid("f0f247a7-ae02-4187-a4a4-e598421e6fd6"), Rule = GetType().ToString() });
                                }

                                var azureCompatibleNode = packageNode.SelectSingleNode("azureCompatible");
                                if (azureCompatibleNode != null)
                                {
                                    manifest.HasAzureCompatibleTag = true;

                                    if (!string.IsNullOrEmpty(azureCompatibleNode.InnerText) && azureCompatibleNode.InnerText.ToLower() == "true")
                                    {
                                        manifest.TaggedAzureCompatible = true;
                                    }
                                }

                                var dependenciesNode = packageNode.SelectSingleNode("dependencies");
                                var dependencyNodeList = dependenciesNode?.SelectNodes("dependency");
                                if (dependencyNodeList != null)
                                {
                                    foreach (XmlNode dependencyNode in dependencyNodeList)
                                    {
                                        var dependencyNodeType = dependencyNode.Attributes["type"];
                                        if (dependencyNodeType != null && !string.IsNullOrEmpty(dependencyNodeType.Value))
                                        {
                                            if (dependencyNodeType.Value.ToLower() == "coreversion")
                                            {
                                                package.CoreVersionDependency = dependencyNode.InnerText;
                                            }
                                        }
                                    }
                                }
                            }

                            var componentsNode = packageNode.SelectSingleNode("components");
                            if (componentsNode == null || !componentsNode.HasChildNodes)
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest must have a list of components.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("996a7832-9ca6-4123-85e7-75efc1d1d57a"), Rule = GetType().ToString() });
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the SQL Azure scanner", exc.Message);
                r.Add(new Models.VerificationMessage { Message = "Package manifest must be properly formatted XML.", MessageType = Models.MessageTypes.SystemError, MessageId = new Guid("5a1585c5-1c08-49b3-885b-6a02f4c59bfa"), Rule = GetType().ToString() });
            }
            return r;
        }
    }
}