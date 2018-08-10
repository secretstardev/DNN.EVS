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
    public class AssemblyNode : ComponentBase, IManifestRule
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

                    if (type.Value != "Assembly") continue;

                    var primaryNode = componentNode.SelectSingleNode("assemblies");
                    if (primaryNode == null) continue;

                    var basePathNode = primaryNode.SelectSingleNode("basePath");
                    if (basePathNode != null)
                    {
                        //removed this check see DNN-1588 by NR on 9/18/14
                        //r.Add(new VerificationMessage { Message = "basePath is not a valid node in component with type 'Assembly'.", MessageType = MessageTypes.Warning, MessageId = new Guid("ae591d52-0ed1-42fb-b6dd-17c482d6df90"), Rule = GetType().ToString()});
                    }

                    var xmlNodeList = primaryNode.SelectNodes("assembly");
                    if (xmlNodeList == null) continue;

                    foreach (XmlNode innerNode in xmlNodeList)
                    {
                        //for assembly nodes we need to check for the "Action" attribute. If the value is "UnRegister" then this node is used to remove a DLL and there for the assembly will not need to be in the package.
                        if (innerNode.Attributes != null && innerNode.Attributes.Count > 0)
                        {
                            var action = innerNode.Attributes["Action"];
                            if (action != null && !string.IsNullOrEmpty(action.Value))
                            {
                                if (action.Value == "UnRegister")
                                {
                                    continue;
                                }
                            }
                        }

                        ProcessNode(r, package, manifest, innerNode);
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the assembly node scanner", exc.Message);
                r.Add(new VerificationMessage() { Message = "An Error occurred while processing Rules.Manifest.Assemblies", MessageType = MessageTypes.SystemError, MessageId = new Guid("511d3578-ef0b-467e-9fa1-ad206909d424") , Rule = GetType().ToString() });
            }

            return r;
        }
    }
}