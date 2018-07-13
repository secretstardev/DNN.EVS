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