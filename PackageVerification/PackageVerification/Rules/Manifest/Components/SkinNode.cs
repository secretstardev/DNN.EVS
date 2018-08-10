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
using PackageVerification.Models;

namespace PackageVerification.Rules.Manifest.Components
{
    public class SkinNode : ComponentBase, IManifestRule
    {
        public List<VerificationMessage> ApplyRule(Package package, Models.Manifest manifest)
        {
            var r = new List<VerificationMessage>();

            try
            {
                if (manifest.ComponentNodes == null)
                    return r;

                foreach (XmlNode componentNode in manifest.ComponentNodes)
                {
                    if (componentNode.Attributes == null) continue;

                    var type = componentNode.Attributes["type"];
                    if (type == null || string.IsNullOrEmpty(type.Value)) continue;

                    if (type.Value != "Skin") continue;

                    var primaryNode = componentNode.SelectSingleNode("skinFiles");
                    if (primaryNode == null) continue;

                    var basePathNode = primaryNode.SelectSingleNode("basePath");
                    if (basePathNode == null)
                    {
                        r.Add(new VerificationMessage { Message = "Each " + primaryNode.Name + " node should have a basePath specified.", MessageType = MessageTypes.Warning, MessageId = new Guid("c50caf3a-96ef-447d-bdd2-d8de337723a6"), Rule = GetType().ToString() });
                    }

                    var skinName = primaryNode.SelectSingleNode("skinName");
                    if (skinName == null || string.IsNullOrEmpty(skinName.InnerText))
                    {
                        r.Add(new VerificationMessage { Message = "ALL components of type Skin must have a skinName specified.", MessageType = MessageTypes.Error, MessageId = new Guid("8678c69b-c2a3-48d9-801c-f18656acba7d"), Rule = GetType().ToString() });
                    }

                    ProcessComponentNode(r, package, manifest, primaryNode, "skinFile");
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the skin node scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.SkinNode", MessageType = MessageTypes.SystemError, MessageId = new Guid("0bf90a74-ab85-4be9-971b-a2584d1e1b1e"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}