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
using System.Xml;

namespace PackageVerification.Rules.Manifest
{
    public class FolderNodeAndChildren : ManifestRulesBase, Models.IManifestRule
    {
        public List<Models.VerificationMessage> ApplyRule(Models.Package package, Models.Manifest manifest)
        {
            var r = new List<Models.VerificationMessage>();
            try
            {
                var xml = manifest.ManifestDoc;

                if (xml != null)
                {
                    var folders = xml.SelectSingleNode("/dotnetnuke/folders");

                    if (folders == null)
                    {
                        r.Add(new Models.VerificationMessage { Message = "Extension manifest must contain a folders node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("274d7fc6-559e-49fc-8cd7-84dbf409745b"), Rule = GetType().ToString() });
                    }
                    else
                    {
                        if (!folders.HasChildNodes)
                        {
                            r.Add(new Models.VerificationMessage { Message = "Extension manifest must contain a folders node with child nodes.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("af254a4c-ad18-4f40-9469-6fd131dc7c8b"), Rule = GetType().ToString() });
                        }

                        foreach (XmlNode folderNode in folders.ChildNodes)
                        {
                            var nameNode = folderNode.SelectSingleNode("name");
                            if (nameNode == null || string.IsNullOrEmpty(nameNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL folder nodes in the manifest must have a name node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("7fd48677-8409-46a7-b790-20ebc90108b0"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = nameNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("d40fe32f-ea79-49c3-802c-f7264fb69a2b"), Rule = GetType().ToString() });
                            }

                            var friendlyNameNode = folderNode.SelectSingleNode("friendlyname");
                            if (friendlyNameNode == null || string.IsNullOrEmpty(friendlyNameNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL folder nodes in the manifest must have a friendly name node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("d6d6dcaf-a5a2-486a-bb30-983b39528ed0"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = friendlyNameNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("671b6736-b686-485b-a269-7be54180517e"), Rule = GetType().ToString() });
                            }

                            var folderNameNode = folderNode.SelectSingleNode("foldername");
                            if (folderNameNode == null || string.IsNullOrEmpty(folderNameNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL folder nodes in the manifest must have a folder name node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("30c30ca0-0cf2-4455-8b1c-88f7d167beb1"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = folderNameNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("fe5b2f25-6572-41f8-86c2-0967efa7d712"), Rule = GetType().ToString() });
                            }

                            var moduleNameNode = folderNode.SelectSingleNode("modulename");
                            if (moduleNameNode == null || string.IsNullOrEmpty(moduleNameNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL folder nodes in the manifest must have a module name node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("3a5ad293-4e08-4a5d-9ba2-b4d4b1d27e39"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = moduleNameNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("52c3d044-89f0-4397-a583-54d66fe738a7"), Rule = GetType().ToString() });
                            }

                            var descriptionNode = folderNode.SelectSingleNode("description");
                            if (descriptionNode == null || string.IsNullOrEmpty(descriptionNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL folder nodes in the manifest must have a description.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("efce2ed8-9748-493b-839d-7c6e370b88dc"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = descriptionNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("93959db4-b7b0-4917-bb76-465173b9dd2b"), Rule = GetType().ToString() });
                            }

                            var versionNode = folderNode.SelectSingleNode("version");
                            if (versionNode == null || string.IsNullOrEmpty(versionNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL folder nodes in the manifest must have a description.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("cd8e3ec7-603e-474f-a85b-6fd8586ad8a5"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = versionNode.InnerText, MessageType = Models.MessageTypes.Info, MessageId = new Guid("aa26db5b-f3a6-4af4-bb88-a9505d247732"), Rule = GetType().ToString() });
                            }

                            r.Add(new Models.VerificationMessage { Message = "Module", MessageType = Models.MessageTypes.Info, MessageId = new Guid("feaf4309-3ef3-4278-b610-b1f78fe3c6a9"), Rule = GetType().ToString() });
                        }
                    }
                }
            }
            catch //(Exception exc)
            {
                r.Add(new Models.VerificationMessage { Message = "Extension manifest must be properly formatted XML.", MessageType = Models.MessageTypes.SystemError, MessageId = new Guid("5a1585c5-1c08-49b3-885b-6a02f4c59bfa"), Rule = GetType().ToString() });
            }
            return r;
        }
    }
}