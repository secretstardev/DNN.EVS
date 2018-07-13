using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using PackageVerification.Models;

namespace PackageVerification.Rules.Manifest.Components
{
    public class CoreLanguageNode : ComponentBase, IManifestRule
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

                    if (type.Value != "CoreLanguage") continue;

                    var primaryNode = componentNode.SelectSingleNode("languageFiles");
                    if (primaryNode == null) continue;

                    var basePathNode = primaryNode.SelectSingleNode("basePath");
                    if (basePathNode != null)
                    {
                        r.Add(new VerificationMessage { Message = "basePath is not a valid node in component with type 'CoreLanguage'.", MessageType = MessageTypes.Warning, MessageId = new Guid("60412b59-530f-4376-84aa-1788affccc75"), Rule = GetType().ToString() });
                    }

                    var code = primaryNode.SelectSingleNode("code");
                    if (code == null || string.IsNullOrEmpty(code.InnerText))
                    {
                        r.Add(new VerificationMessage { Message = "ALL components of type Core Language must have a code specified.", MessageType = MessageTypes.Error, MessageId = new Guid("3c7929d8-51ec-4320-aa42-1dac92a83d3d"), Rule = GetType().ToString() });
                    }

                    var displayName = primaryNode.SelectSingleNode("displayName");
                    if (displayName == null || string.IsNullOrEmpty(displayName.InnerText))
                    {
                        r.Add(new VerificationMessage { Message = "ALL components of type Core Language must have a displayName specified.", MessageType = MessageTypes.Error, MessageId = new Guid("f241a413-b92f-47ce-b652-5005cb7b912b"), Rule = GetType().ToString() });
                    }

                    var fallback = primaryNode.SelectSingleNode("fallback");
                    if (fallback == null || string.IsNullOrEmpty(fallback.InnerText))
                    {
                        r.Add(new VerificationMessage { Message = "ALL components of type Core Language must have a fallback specified.", MessageType = MessageTypes.Error, MessageId = new Guid("41321a88-2aaa-4cd4-825d-a9a2c40428bf"), Rule = GetType().ToString() });
                    }

                    ProcessComponentNode(r, package, manifest, primaryNode, "languageFile");
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the core language node scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.CoreLanguageNode", MessageType = MessageTypes.SystemError, MessageId = new Guid("3024cea7-5cf7-44f6-8d11-b973d51eb869"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}