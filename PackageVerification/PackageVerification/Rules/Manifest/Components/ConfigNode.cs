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
    public class ConfigNode : ManifestRulesBase, Models.IManifestRule
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

                    if (type.Value != "Config") continue;

                    var configNode = componentNode.SelectSingleNode("config");
                    if (configNode == null) continue;

                    var configFileNode = configNode.SelectSingleNode("configFile");
                    if (configFileNode == null)
                    {
                        r.Add(new Models.VerificationMessage { Message = "All config nodes must have a configFile specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("a8178b28-92da-4933-b9c5-b7396ee97561"), Rule = GetType().ToString() });
                    }
                    else
                    {
                        var configFile = configFileNode.InnerText;
                        r.Add(new Models.VerificationMessage { Message = "Installing this extension will result in changes to the " + configFile + " file.", MessageType = Models.MessageTypes.Info, MessageId = new Guid("c47e6316-4793-4330-911c-6ad434ac1a73"), Rule = GetType().ToString() });
                    }

                    var installNode = configNode.SelectSingleNode("install");
                    if (installNode == null)
                    {
                        r.Add(new Models.VerificationMessage { Message = "All 'config' nodes must have a 'install' node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("54c2e281-07f6-4fd7-9f6d-2995514d861b"), Rule = GetType().ToString() });
                    }
                    else
                    {
                        var configurationNode = installNode.SelectSingleNode("configuration");
                        if (configurationNode == null)
                        {
                            r.Add(new Models.VerificationMessage { Message = "All 'config/install' nodes must have a 'configuration' node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("25a7da28-953a-4f1b-8c00-e7c0f4ee8b3e"), Rule = GetType().ToString() });
                        }
                        else
                        {
                            var nodesNode = configurationNode.SelectSingleNode("nodes");
                            if (nodesNode == null)
                            {
                                r.Add(new Models.VerificationMessage { Message = "All 'config/install/configuration' nodes must have a 'nodes' node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("072f49a1-87ba-4996-bd38-629b6fe9cee8"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                var nodeNodes = nodesNode.SelectNodes("node");
                                if (nodeNodes != null)
                                {
                                    foreach (XmlNode nodeNode in nodeNodes)
                                    {
                                        if (nodeNode.Attributes != null && nodeNode.Attributes.Count > 0)
                                        {
                                            var path = nodeNode.Attributes["path"];
                                            if (path == null || string.IsNullOrEmpty(path.Value))
                                            {
                                                r.Add(new Models.VerificationMessage { Message = "All 'config/install/configuration/nodes/node' nodes must have a 'path' attribute.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("e796ad35-e39d-4921-b92d-c94524ea4430"), Rule = GetType().ToString() });
                                            }

                                            var action = nodeNode.Attributes["action"];
                                            if (action == null || string.IsNullOrEmpty(action.Value))
                                            {
                                                r.Add(new Models.VerificationMessage { Message = "All 'config/install/configuration/nodes/node' nodes must have a 'action' attribute.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("822d6ff2-7feb-439c-b232-b9c3b9177f24"), Rule = GetType().ToString() });
                                            }
                                            else
                                            {
                                                if (action.Value.ToLower() != "remove")
                                                {
                                                    var collision = nodeNode.Attributes["collision"];
                                                    if (collision == null || string.IsNullOrEmpty(collision.Value))
                                                    {
                                                        r.Add(new Models.VerificationMessage { Message = "All 'config/install/configuration/nodes/node' nodes must have a 'collision' attribute unless the action node is set to 'remove'.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("243e15fa-1aff-4ee6-b19d-aeeb75768b9c"), Rule = GetType().ToString() });
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            r.Add(new Models.VerificationMessage { Message = "Each 'config/install/configuration/nodes/node' node must have a path, action and collision attribute.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("5da6d43b-7079-46fe-9800-4f180740ec7e"), Rule = GetType().ToString() });
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var uninstallNode = configNode.SelectSingleNode("uninstall");
                    if (uninstallNode == null)
                    {
                        r.Add(new Models.VerificationMessage { Message = "All 'config' nodes must have a 'uninstall' node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("b797fa28-bb51-4b40-80bc-1137a217be9f"), Rule = GetType().ToString() });
                    }
                    else
                    {
                        var configurationNode = uninstallNode.SelectSingleNode("configuration");
                        if (configurationNode == null)
                        {
                            r.Add(new Models.VerificationMessage { Message = "All 'config/uninstall' nodes must have a 'configuration' node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("6727051d-4e36-4709-b6e6-850e2a118030"), Rule = GetType().ToString() });
                        }
                        else
                        {
                            var nodesNode = configurationNode.SelectSingleNode("nodes");
                            if (nodesNode == null)
                            {
                                r.Add(new Models.VerificationMessage { Message = "All 'config/uninstall/configuration' nodes must have a 'nodes' node.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("59a2a2af-6071-4e2d-89a4-1d41b176dba5"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                var nodeNodes = nodesNode.SelectNodes("node");
                                if (nodeNodes != null)
                                {
                                    foreach (XmlNode nodeNode in nodeNodes)
                                    {
                                        if (nodeNode.Attributes != null && nodeNode.Attributes.Count > 0)
                                        {
                                            var path = nodeNode.Attributes["path"];
                                            if (path == null || string.IsNullOrEmpty(path.Value))
                                            {
                                                r.Add(new Models.VerificationMessage { Message = "All 'config/uninstall/configuration/nodes/node' nodes must have a 'path' attribute.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("83da185a-66d1-41eb-80d9-357ed83e71e7"), Rule = GetType().ToString() });
                                            }

                                            var action = nodeNode.Attributes["action"];
                                            if (action == null || string.IsNullOrEmpty(action.Value))
                                            {
                                                r.Add(new Models.VerificationMessage { Message = "All 'config/uninstall/configuration/nodes/node' nodes must have a 'action' attribute.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("3a53b9b4-32f2-4e2e-8f09-ea81d7ced1c5"), Rule = GetType().ToString() });
                                            }
                                        }
                                        else
                                        {
                                            r.Add(new Models.VerificationMessage { Message = "Each 'config/uninstall/configuration/nodes/node' node must have a path and action attribute.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("640b6762-7451-47e2-a8e1-bd0e5d39abf8"), Rule = GetType().ToString() });
                                        }
                                    }
                                }
                            }
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