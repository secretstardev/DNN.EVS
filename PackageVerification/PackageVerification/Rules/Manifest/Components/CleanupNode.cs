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
using System.Xml;

namespace PackageVerification.Rules.Manifest.Components
{
    public class CleanupNode : ManifestRulesBase, Models.IManifestRule
    {
        public List<Models.VerificationMessage> ApplyRule(Models.Package package, Models.Manifest manifest)
        {
            var r = new List<Models.VerificationMessage>();
            try
            {
                if (manifest.ComponentNodes == null)
                    return r;
                foreach (XmlNode componentNode in manifest.ComponentNodes)
                {
                    if (componentNode.Attributes == null) continue;

                    var type = componentNode.Attributes["type"];
                    if (type == null || string.IsNullOrEmpty(type.Value)) continue;

                    if (type.Value != "Cleanup") continue;

                    var path = package.ExtractedPath;
                    var version = componentNode.Attributes["version"];
                    if (version == null || string.IsNullOrEmpty(version.Value))
                    {
                        r.Add(new Models.VerificationMessage { Message = "Each component node with the type of 'Cleanup' must have a version attribute.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("cd88ea76-d98f-4d42-9c5a-4deb6dcf6030"), Rule = GetType().ToString() });
                    }

                    var strFileName = "";

                    var fileName = componentNode.Attributes["fileName"];
                    if (fileName != null && !string.IsNullOrEmpty(fileName.Value))
                    {
                        strFileName = fileName.Value;

                        if (strFileName != "")
                        {
                            var fullFilePath = System.IO.Path.Combine(path, strFileName);
                            manifest.Files.Add(fullFilePath.ToLower());
                        }
                    }
                    else
                    {
                        var cleanupFilesNode = componentNode.SelectSingleNode("files");

                        if (cleanupFilesNode != null)
                        {
                            var clanupFilesBasePath = cleanupFilesNode.SelectSingleNode("basePath");
                            if (clanupFilesBasePath == null || string.IsNullOrEmpty(clanupFilesBasePath.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL files nodes should have a basePath specified.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("0621ac6e-fbc2-4459-b7c5-b849401b36b2"), Rule = GetType().ToString() });
                            }

                            var cleanupFileNodes = componentNode.SelectNodes("file");
                            if (cleanupFileNodes != null)
                            {
                                foreach (XmlNode cleanupFileNode in cleanupFileNodes)
                                {
                                    var nameNode = cleanupFileNode.SelectSingleNode("name");
                                    if(nameNode == null || string.IsNullOrEmpty(nameNode.Value))
                                    {
                                        r.Add(new Models.VerificationMessage { Message = "Each file node must have a 'name' node with in it.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("2feeaa18-6332-4092-bbff-24a792b52294"), Rule = GetType().ToString() });
                                    }
                                }
                            }
                            else
                            {
                                r.Add(new Models.VerificationMessage { Message = "Each files node must have a at least one file node within it.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("3ce59978-ca73-4f77-97d7-aef5bd65f09e"), Rule = GetType().ToString() });
                            }
                        }
                        else
                        {
                            r.Add(new Models.VerificationMessage { Message = "Each component node with the type of 'Cleanup' must have a fileName attribute or a 'files' child node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("b79b60f5-1fce-4cdc-b1a1-c6abc3ef82e9"), Rule = GetType().ToString() });   
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the cleanup node scanner", exc.Message);
                r.Add(new Models.VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.CleanupNode", MessageType = Models.MessageTypes.SystemError, MessageId = new Guid("1ab5a0a2-e7c0-41d7-89f2-44395df784f9"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}