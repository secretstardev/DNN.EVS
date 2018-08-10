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
    public class SkinObjectNode : ManifestRulesBase, Models.IManifestRule
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

                    if (type.Value != "SkinObject") continue;

                    var moduleControls = componentNode.SelectNodes("moduleControl");
                    if (moduleControls == null || moduleControls.Count <= 0)
                    {
                        r.Add(new Models.VerificationMessage { Message = "ALL Skin Object Definitions must have at least one module control specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("070cad12-9ee1-4453-9174-5523e81c5e73"), Rule = GetType().ToString() });
                    }
                    else
                    {
                        foreach (XmlNode ctl in moduleControls)
                        {
                            var controlKey = ctl.SelectSingleNode("controlKey");
                            if (controlKey == null || string.IsNullOrEmpty(controlKey.InnerText))
                            {
                                r.Add(new Models.VerificationMessage { Message = "ALL Module controls must have a control key specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("d05bc48a-412b-4bae-905b-87c4742bed84"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                var controlSrc = ctl.SelectSingleNode("controlSrc");
                                if (controlSrc == null || string.IsNullOrEmpty(controlSrc.InnerText))
                                {
                                    r.Add(new Models.VerificationMessage { Message = "ALL Module controls must have a control source specified.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("210ce338-4c0b-40f3-aefc-2688b648ef22"), Rule = GetType().ToString() });
                                }
                                else
                                {
                                    var supportsPartialRendering = ctl.SelectSingleNode("supportsPartialRendering");
                                    if (supportsPartialRendering == null || string.IsNullOrEmpty(supportsPartialRendering.InnerText)) // || supportsPartialRendering.InnerText.ToLowerInvariant() == "false")
                                    {
                                        r.Add(new Models.VerificationMessage { Message = "A module control which does not specify supportsPartialRendering has been detected (" + controlSrc.InnerText + ").", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("9e9bfd26-ac1d-46c4-bdad-7b293d1cf57d"), Rule = GetType().ToString() });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the Skin Object node scanner", exc.Message);
                r.Add(new Models.VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.SkinObjectNode", MessageType = Models.MessageTypes.SystemError, MessageId = new Guid("bd9af978-f01e-432a-ab12-12322564182c"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}