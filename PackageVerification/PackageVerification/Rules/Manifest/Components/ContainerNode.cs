using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using PackageVerification.Models;

namespace PackageVerification.Rules.Manifest.Components
{
    public class ContainerNode : ComponentBase, IManifestRule
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

                    if (type.Value != "Container") continue;

                    var primaryNode = componentNode.SelectSingleNode("containerFiles");
                    if (primaryNode == null) continue;

                    var basePathNode = primaryNode.SelectSingleNode("basePath");
                    if (basePathNode == null)
                    {
                        r.Add(new VerificationMessage { Message = "Each " + primaryNode.Name + " node should have a basePath specified.", MessageType = MessageTypes.Warning, MessageId = new Guid("30f3a439-8173-4f5a-ac08-ffda8f8aa3b6"), Rule = GetType().ToString() });
                    }

                    var containerNameNode = primaryNode.SelectSingleNode("containerName");
                    if (containerNameNode == null || string.IsNullOrEmpty(containerNameNode.InnerText))
                    {
                        r.Add(new VerificationMessage { Message = "Each " + primaryNode.Name + " node should have a containerName specified.", MessageType = MessageTypes.Error, MessageId = new Guid("b9b12b5e-ce02-4812-9ca4-c1ba767fbeff"), Rule = GetType().ToString() });
                    }

                    ProcessComponentNode(r, package, manifest, primaryNode, "containerFile");
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the container node scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.ContainerNode", MessageType = MessageTypes.SystemError, MessageId = new Guid("419bb7bf-e227-401d-8462-7982683ca703"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}