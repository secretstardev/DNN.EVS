using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using PackageVerification.Models;

namespace PackageVerification.Rules.Manifest.Components
{
    public class FileNode : ComponentBase, IManifestRule
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

                            if (type.Value != "File") continue;

                            var primaryNode = componentNode.SelectSingleNode("files");
                            if (primaryNode == null) continue;

                            var basePathNode = primaryNode.SelectSingleNode("basePath");
                            if (basePathNode == null)
                            {
                                r.Add(new VerificationMessage { Message = "Each " + primaryNode.Name + " node should have a basePath specified.", MessageType = MessageTypes.Warning, MessageId = new Guid("2ddfd5a6-85c6-45df-ad6a-91d21290d34a"), Rule = GetType().ToString() });
                            }

                            ProcessComponentNode(r, package, manifest, primaryNode, "file");
                        }
                        break;
                    case ManifestTypes.Module:
                        var folderNodes = manifest.ManifestDoc.SelectNodes("dotnetnuke/folders/folder");

                        if (folderNodes == null || folderNodes.Count <= 0)
                        {
                            r.Add(new VerificationMessage{Message = "ALL components of type Module must have at least one folder node specified.",MessageType = MessageTypes.Error,MessageId = new Guid("6c7cb47b-2c63-43ed-8d15-ac8f7567d74b"),Rule = GetType().ToString()});
                        }
                        else
                        {
                            foreach (XmlNode folderNode in folderNodes)
                            {
                                var primaryNode = folderNode.SelectSingleNode("files");
                                if (primaryNode == null) continue;
                                
                                ProcessComponentNode(r, package, manifest, primaryNode, "file");
                            }
                        }
                        break;
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the file node scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.FileNode", MessageType = MessageTypes.SystemError, MessageId = new Guid("551646a4-3a12-470f-a51d-98d2343b8de7"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}