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
    public class PackageOwner : ManifestRulesBase, Models.IManifestRule
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

                    if (packages != null)
                        foreach (XmlNode packageNode in packages.ChildNodes)
                        {
                            var ownerNode = packageNode.SelectSingleNode("owner");
                            if (ownerNode == null)
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest must have an owner.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("13c5a640-56ed-484e-956c-ebad86ce8399"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                var name = ownerNode.SelectSingleNode("name");
                                if (name == null || string.IsNullOrEmpty(name.InnerText))
                                {
                                    r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest should have an owner name.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("22caecf4-419f-4f7a-92bb-69e644762941"), Rule = GetType().ToString() });
                                }
                                var organization = ownerNode.SelectSingleNode("organization");
                                if (organization == null || string.IsNullOrEmpty(organization.InnerText))
                                {
                                    r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest should have an owner organization.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("cbd22b56-778c-43d5-bebc-89e44b99684b"), Rule = GetType().ToString() });
                                }
                                var url = ownerNode.SelectSingleNode("url");
                                if (url == null || string.IsNullOrEmpty(url.InnerText))
                                {
                                    r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest should have an owner URL.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("aa1d517b-3655-4f92-9332-9544a9f54eed"), Rule = GetType().ToString() });
                                }
                                var email = ownerNode.SelectSingleNode("email");
                                if (email == null || string.IsNullOrEmpty(email.InnerText))
                                {
                                    r.Add(new Models.VerificationMessage { Message = "ALL packages in the manifest should have an owner Email.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("34db8679-d5b2-4722-a17d-d555eaf597ae"), Rule = GetType().ToString() });
                                }
                            }
                        }
                }
            }
            catch (Exception)
            {
                r.Add(new Models.VerificationMessage { Message = "There was a problem parsing the package owner from the xml manifest.", MessageType = Models.MessageTypes.SystemError, MessageId = new Guid("f35eea0a-2841-4c8e-978c-bfcb7fa0ba42"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}