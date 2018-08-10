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
    public class DashboardControlNode : ManifestRulesBase, Models.IManifestRule
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

                    if (type.Value != "DashboardControl") continue;

                    var dashboardControlNodes = componentNode.SelectNodes("dashboardControl");
                    if (dashboardControlNodes == null || dashboardControlNodes.Count <= 0)
                    {
                        r.Add(new Models.VerificationMessage { Message = "ALL Dashboard ControlDefinitions must have at least one dashboardControl node specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("a56f4b2d-09b0-406e-a1dc-a67e2b646cd4"), Rule = GetType().ToString() });
                    }
                    else
                    {
                        foreach (XmlNode ctl in dashboardControlNodes)
                        {
                            var keyNode = ctl.SelectSingleNode("key");
                            if (keyNode == null || string.IsNullOrEmpty(keyNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL dashboard control nodes must have a type node specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("6a6ba22f-3b20-44e9-9717-c63a52131e37"), Rule = GetType().ToString() });
                            }

                            var src = ctl.SelectSingleNode("src");
                            if (src == null || string.IsNullOrEmpty(src.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL dashboard control nodes must have a src node specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("f9b83213-46bd-4ed7-a569-493faab3ccef"), Rule = GetType().ToString() });
                            }

                            var localResources = ctl.SelectSingleNode("localResources");
                            if (localResources == null || string.IsNullOrEmpty(localResources.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL dashboard control nodes must have a local resources node specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("d156bebb-0073-4384-97a3-6d47e2a9519e"), Rule = GetType().ToString() });
                            }

                            var controllerClass = ctl.SelectSingleNode("controllerClass");
                            if (controllerClass == null || string.IsNullOrEmpty(controllerClass.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL dashboard control nodes must have a controller class node specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("10253a22-66c6-4259-9179-83fa34b760ae"), Rule = GetType().ToString() });
                            }

                            var isEnabled = ctl.SelectSingleNode("isEnabled");
                            if (isEnabled == null || string.IsNullOrEmpty(isEnabled.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL dashboard control nodes must have a isEnabled node specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("fb5055df-c353-4bef-953d-87e13c2852b8"), Rule = GetType().ToString() });
                            }

                            var viewOrder = ctl.SelectSingleNode("viewOrder");
                            if (viewOrder == null || string.IsNullOrEmpty(viewOrder.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL dashboard control nodes must have a viewOrder node specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("ee95a3a9-16ca-40c5-a0a3-b0980a3803eb"), Rule = GetType().ToString() });
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the Dashboard Control node scanner", exc.Message);
                r.Add(new Models.VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.DashboardControlNode", MessageType = Models.MessageTypes.SystemError, MessageId = new Guid("38d83abe-546a-432f-8253-9e0b382690f9"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}