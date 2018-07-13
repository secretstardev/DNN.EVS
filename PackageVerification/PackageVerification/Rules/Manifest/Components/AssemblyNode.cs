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