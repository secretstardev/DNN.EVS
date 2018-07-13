using System;
using System.Collections.Generic;
using PackageVerification.Models;

namespace PackageVerification.Rules.Manifest
{
    public class CorrectTypeAndVersion : ManifestRulesBase, IManifestRule
    {
        public List<VerificationMessage> ApplyRule(Package package, Models.Manifest manifest)
        {
            var r = new List<VerificationMessage>();
            try
            {
                var xml = manifest.ManifestDoc;

                if (xml != null)
                {
                    var dotnetnuke = xml.SelectSingleNode("/dotnetnuke");
                    if (dotnetnuke == null)
                    {
                        //TODO: this is not true, legacy language packs could have a root node of languagepack
                        r.Add(new VerificationMessage { Message = "Package manifest must contain a dotnetnuke node.", MessageType = MessageTypes.Error, MessageId = new Guid("B90BA8D2-25A7-4BF0-A7BD-F6ECC6C4F0FF"), Rule = GetType().ToString() });
                    }
                    else
                    {
                        if (dotnetnuke.Attributes == null)
                        {
                            r.Add(new VerificationMessage { Message = "DotNetNuke node should have two attributes version and type.", MessageType = MessageTypes.Error, MessageId = new Guid("36d013e8-4d9a-4127-8f9f-3594b4ccf093"), Rule = GetType().ToString() });
                        }
                        else
                        {
                            var versionAttr = dotnetnuke.Attributes["version"];
                            if (versionAttr == null || string.IsNullOrEmpty(versionAttr.Value))
                            {
                                //NOTE: from the looks of it, this might not be true.
                                r.Add(new VerificationMessage { Message = "DotNetNuke node must contain a version attribute.", MessageType = MessageTypes.Error, MessageId = new Guid("7842d95e-cfa3-4273-96b5-3d998328a90e"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                double version;
                                double.TryParse(versionAttr.Value, out version);

                                if (!version.Equals(manifest.ManifestVersion()))
                                {
                                    r.Add(new VerificationMessage { Message = "The value of the version attribute doesn't match the detected manifest version.", MessageType = MessageTypes.Error, MessageId = new Guid("07377574-6fba-4141-beb4-e40d5ab8f192"), Rule = GetType().ToString() });
                                }
                            }

                            var typeAttr = dotnetnuke.Attributes["type"];
                            if (typeAttr == null || string.IsNullOrEmpty(typeAttr.Value))
                            {
                                r.Add(new VerificationMessage { Message = "DotNetNuke node must contain a type attribute.", MessageType = MessageTypes.Error, MessageId = new Guid("3b61cb99-039c-4af1-8c1b-37c0845bc5df"), Rule = GetType().ToString() });
                            }
                            else
                            {
                                var type = typeAttr.Value;
                                if (manifest.ManifestVersion() >= 5.0)
                                {
                                    //If version is >= 5.0 then the type must be package
                                    if (type.ToLower() == "package")
                                    {
                                        manifest.ManifestType = ManifestTypes.Package;
                                    }
                                    else
                                    {
                                        r.Add(new VerificationMessage { Message = "When the version is >= 5.0, the type attribute value should typically be 'Package'.", MessageType = MessageTypes.Warning, MessageId = new Guid("09b0e52a-a475-44d0-a66b-a4cd5ef182e7"), Rule = GetType().ToString() });
                                    }
                                }
                                else if (manifest.ManifestVersion() < 5.0)
                                {
                                    if (type.ToLower() == "module")
                                    {
                                        manifest.ManifestType = ManifestTypes.Module;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                r.Add(new VerificationMessage { Message = "There was an error while attempting to determine the package type and version.", MessageType = MessageTypes.Error, MessageId = new Guid("550c93f8-1eec-4196-a41e-ec160cfa325f"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}