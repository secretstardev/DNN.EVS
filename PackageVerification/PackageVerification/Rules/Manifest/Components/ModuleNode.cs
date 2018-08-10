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
    public class ModuleNode : ManifestRulesBase, IManifestRule
    {
        public List<VerificationMessage> ApplyRule(Package package, Models.Manifest manifest)
        {
            var r = new List<VerificationMessage>();
            try
            {
                switch (manifest.ManifestType)
                {
                    case ManifestTypes.Package:
                        if (manifest.ComponentNodes == null)
                            return r;
                        foreach (XmlNode componentNode in manifest.ComponentNodes)
                        {
                            if (componentNode.Attributes == null) continue;

                            var type = componentNode.Attributes["type"];
                            if (type == null || string.IsNullOrEmpty(type.Value)) continue;

                            if (type.Value != "Module") continue;

                            var desktopModuleNodes = componentNode.SelectNodes("desktopModule");
                            if (desktopModuleNodes == null) continue;

                            foreach (XmlNode desktopModuleNode in desktopModuleNodes)
                            {
                                var moduleName = desktopModuleNode.SelectSingleNode("moduleName");
                                if (moduleName == null || string.IsNullOrEmpty(moduleName.InnerText))
                                {
                                    r.Add(new VerificationMessage { Message = "ALL components of type Module must have a moduleName specified.", MessageType = MessageTypes.Error, MessageId = new Guid("2ac58e69-1ca8-4ea8-b86b-a3a0c3edbf5c"), Rule = GetType().ToString() });
                                }
                                var foldername = desktopModuleNode.SelectSingleNode("foldername");
                                if (foldername == null || string.IsNullOrEmpty(foldername.InnerText))
                                {
                                    r.Add(new VerificationMessage { Message = "ALL components of type Module must have a foldername specified.", MessageType = MessageTypes.Error, MessageId = new Guid("0c0ab7a5-8119-4421-9f93-4f87eed3ca1c"), Rule = GetType().ToString() });
                                }

                                var moduleDefinitions = desktopModuleNode.SelectNodes("moduleDefinitions/moduleDefinition");

                                if (moduleDefinitions == null ||
                                    moduleDefinitions.Count <= 0)
                                {
                                    r.Add(new VerificationMessage { Message = "ALL components of type Module must have at least one Module Definition specified.", MessageType = MessageTypes.Error, MessageId = new Guid("d1373d8a-fc8f-459b-9f56-d8fd979eadac"), Rule = GetType().ToString() });
                                }
                                else
                                {
                                    foreach (XmlNode md in moduleDefinitions)
                                    {
                                        var friendlyName = md.SelectSingleNode("friendlyName");
                                        if (friendlyName == null || string.IsNullOrEmpty(friendlyName.InnerText))
                                        {
                                            r.Add(new VerificationMessage { Message = "ALL Module Definitions must have a friendly name specified.", MessageType = MessageTypes.Error, MessageId = new Guid("58a29289-31db-4b76-b716-1292949bf773"), Rule = GetType().ToString() });
                                        }
                                        var moduleControls = md.SelectNodes("moduleControls/moduleControl");
                                        if (moduleControls == null || moduleControls.Count <= 0)
                                        {
                                            r.Add(new VerificationMessage { Message = "ALL Module Definitions must have at least one module control specified.", MessageType = MessageTypes.Error, MessageId = new Guid("bc0f97b4-d0a5-409d-93b2-62543a878614"), Rule = GetType().ToString() });
                                        }
                                        else
                                        {
                                            foreach (XmlNode ctl in moduleControls)
                                            {
                                                var controlSrc = ctl.SelectSingleNode("controlSrc");
                                                if (controlSrc == null || string.IsNullOrEmpty(controlSrc.InnerText))
                                                {
                                                    r.Add(new VerificationMessage { Message = "ALL Module controls must have a control source specified.", MessageType = MessageTypes.Error, MessageId = new Guid("67326c8d-26c8-48db-9092-4dd405a80d05"), Rule = GetType().ToString() });
                                                }
                                                else
                                                {
                                                    var supportsPartialRendering = ctl.SelectSingleNode("supportsPartialRendering");
                                                    if (supportsPartialRendering == null || string.IsNullOrEmpty(supportsPartialRendering.InnerText)) // || supportsPartialRendering.InnerText.ToLowerInvariant() == "false")
                                                    {
                                                        r.Add(new VerificationMessage { Message = "A module control which does not specify supportsPartialRendering has been detected (" + controlSrc.InnerText + ").", MessageType = MessageTypes.Warning, MessageId = new Guid("fd9762a0-2f70-41bf-8b27-96ef38e5880a"), Rule = GetType().ToString() });
                                                    }
                                                    var controlType = ctl.SelectSingleNode("controlType");
                                                    if (controlType == null || string.IsNullOrEmpty(controlType.InnerText))
                                                    {
                                                        r.Add(new VerificationMessage { Message = "A module control which does not specify controlType has been detected (" + controlSrc.InnerText + ").", MessageType = MessageTypes.Warning, MessageId = new Guid("9a22a25c-b8b2-4ee1-9cac-0a755b494838"), Rule = GetType().ToString() });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ManifestTypes.Module:
                        var folderNodes = manifest.ManifestDoc.SelectNodes("dotnetnuke/folders/folder");

                        if (folderNodes == null || folderNodes.Count <= 0)
                        {
                            r.Add(new VerificationMessage { Message = "ALL components of type Module must have at least one folder node specified.", MessageType = MessageTypes.Error, MessageId = new Guid("6c7cb47b-2c63-43ed-8d15-ac8f7567d74b"), Rule = GetType().ToString() });
                        }
                        else
                        {
                            foreach (XmlNode folderNode in folderNodes)
                            {
                                var moduleNodes = folderNode.SelectNodes("modules/module");
                            
                                if (moduleNodes == null || moduleNodes.Count <= 0)
                                {
                                    r.Add(new VerificationMessage { Message = "ALL components of type Module must have at least one module node specified.", MessageType = MessageTypes.Error, MessageId = new Guid("973f01e3-bcaf-49ee-a0e2-a143ec4ea369"), Rule = GetType().ToString() });
                                }
                                else
                                {
                                    foreach (XmlNode mn in moduleNodes)
                                    {
                                        var friendlyName = mn.SelectSingleNode("friendlyname");
                                        if (friendlyName == null || string.IsNullOrEmpty(friendlyName.InnerText))
                                        {
                                            r.Add(new VerificationMessage { Message = "All module definitions must have a friendly name specified.", MessageType = MessageTypes.Error, MessageId = new Guid("58a29289-31db-4b76-b716-1292949bf773"), Rule = GetType().ToString() });
                                        }
                                        var cacheTime = mn.SelectSingleNode("cachetime");
                                        if (cacheTime == null || string.IsNullOrEmpty(cacheTime.InnerText))
                                        {
                                            r.Add(new VerificationMessage { Message = "Module definitions should have a cache time specified.", MessageType = MessageTypes.Warning, MessageId = new Guid("b2f5c33e-f38e-49e9-9472-24279f27d0e6"), Rule = GetType().ToString() });
                                        }
                                        var moduleControls = mn.SelectNodes("controls/control");
                                        if (moduleControls == null || moduleControls.Count <= 0)
                                        {
                                            r.Add(new VerificationMessage { Message = "All module definitions must have at least one module control specified.", MessageType = MessageTypes.Error, MessageId = new Guid("bc0f97b4-d0a5-409d-93b2-62543a878614"), Rule = GetType().ToString() });
                                        }
                                        else
                                        {
                                            foreach (XmlNode ctl in moduleControls)
                                            {
                                                var title = mn.SelectSingleNode("title");
                                                if (title == null || string.IsNullOrEmpty(title.InnerText))
                                                {
                                                    r.Add(new VerificationMessage { Message = "All module control definitions should have a title specified.", MessageType = MessageTypes.Warning, MessageId = new Guid("84f217b5-2b6b-4154-a194-04ba9115341a"), Rule = GetType().ToString() });
                                                }

                                                var src = ctl.SelectSingleNode("src");
                                                if (src == null || string.IsNullOrEmpty(src.InnerText))
                                                {
                                                    r.Add(new VerificationMessage { Message = "ALL Module controls must have a source specified.", MessageType = MessageTypes.Error, MessageId = new Guid("67326c8d-26c8-48db-9092-4dd405a80d05"), Rule = GetType().ToString() });
                                                }
                                                else
                                                {
                                                    var supportsPartialRendering = ctl.SelectSingleNode("supportspartialrendering");
                                                    if (supportsPartialRendering == null || string.IsNullOrEmpty(supportsPartialRendering.InnerText)) // || supportsPartialRendering.InnerText.ToLowerInvariant() == "false")
                                                    {
                                                        r.Add(new VerificationMessage { Message = "A module control which does not specify supportsPartialRendering has been detected (" + src.InnerText + ").", MessageType = MessageTypes.Warning, MessageId = new Guid("fd9762a0-2f70-41bf-8b27-96ef38e5880a"), Rule = GetType().ToString() });
                                                    }
                                                    var controlType = ctl.SelectSingleNode("type");
                                                    if (controlType == null || string.IsNullOrEmpty(controlType.InnerText))
                                                    {
                                                        r.Add(new VerificationMessage { Message = "A module control which does not specify controlType has been detected (" + src.InnerText + ").", MessageType = MessageTypes.Warning, MessageId = new Guid("9a22a25c-b8b2-4ee1-9cac-0a755b494838"), Rule = GetType().ToString() });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        break;
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the module node scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.ModuleNode", MessageType = MessageTypes.SystemError, MessageId = new Guid("551646a4-3a12-470f-a51d-98d2343b8de7"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}