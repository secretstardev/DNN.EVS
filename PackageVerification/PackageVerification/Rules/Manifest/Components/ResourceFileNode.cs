using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using PackageVerification.Models;

namespace PackageVerification.Rules.Manifest.Components
{
    public class ResourceFileNode : ComponentBase, IManifestRule
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

                    if (type.Value != "ResourceFile") continue;

                    var primaryNode = componentNode.SelectSingleNode("resourceFiles");
                    if (primaryNode == null) continue;

                    var basePathNode = primaryNode.SelectSingleNode("basePath");
                    if (basePathNode == null)
                    {
                        r.Add(new VerificationMessage {Message ="ALL resource file nodes should have a basePath specified.", MessageType = MessageTypes.Warning, MessageId =new Guid("121977cf-33cc-4abc-866d-8829b8714a29"), Rule = GetType().ToString()});
                    }

                    ProcessComponentNode(r, package, manifest, primaryNode, "resourceFile");
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the resource file scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.ResourceFileNode", MessageType = MessageTypes.SystemError, MessageId = new Guid("9378b453-6b60-45d8-a39f-fde5f9fce88a"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}