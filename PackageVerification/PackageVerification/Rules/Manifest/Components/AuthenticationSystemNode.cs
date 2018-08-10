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
    public class AuthenticationSystemNode : ManifestRulesBase, Models.IManifestRule
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

                    if (type.Value != "AuthenticationSystem") continue;

                    var authenticationServiceNodes = componentNode.SelectNodes("authenticationService");
                    if (authenticationServiceNodes == null || authenticationServiceNodes.Count <= 0)
                    {
                        r.Add(new Models.VerificationMessage { Message = "ALL Authentication System Definitions must have at least one authenticationService specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("d9bc01ff-0cc1-4b08-9750-1f9ad2d6c27e"), Rule = GetType().ToString() });
                    }
                    else
                    {
                        foreach (XmlNode ctl in authenticationServiceNodes)
                        {
                            var typeNode = ctl.SelectSingleNode("type");
                            if (typeNode == null || string.IsNullOrEmpty(typeNode.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL authentication service nodes must have a type specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("297f1026-1d9a-4d06-896b-0deb0e0c17be"), Rule = GetType().ToString() });
                            }

                            var settingsControlSrc = ctl.SelectSingleNode("settingsControlSrc");
                            if (settingsControlSrc == null || string.IsNullOrEmpty(settingsControlSrc.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL authentication service nodes must have a settings control source specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("00814677-1649-4984-a815-f3920e895a7a"), Rule = GetType().ToString() });
                            }

                            var loginControlSrc = ctl.SelectSingleNode("loginControlSrc");
                            if (loginControlSrc == null || string.IsNullOrEmpty(loginControlSrc.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL authentication service nodes must have a login control source specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("82942d23-6cae-4976-bd74-fae3fca9287f"), Rule = GetType().ToString() });
                            }

                            var logoffControlSrc = ctl.SelectSingleNode("logoffControlSrc");
                            if (logoffControlSrc == null || string.IsNullOrEmpty(logoffControlSrc.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL authentication service nodes must have a logoff control source specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("3f4ec2b6-3e61-4c29-b90f-44cb9ee153e9"), Rule = GetType().ToString() });
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the Authentication System node scanner", exc.Message);
                r.Add(new Models.VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.AuthenticationSystemNode", MessageType = Models.MessageTypes.SystemError, MessageId = new Guid("c8458f8e-91b2-4c81-ae02-e1b3a18aec8d"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}