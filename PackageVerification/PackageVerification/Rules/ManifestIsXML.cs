using System;
using System.Collections.Generic;

namespace PackageVerification.Rules
{
    public class ManifestIsXML : Models.IPackageRule
    {
        public List<Models.VerificationMessage> ApplyRule(Models.Package package)
        {
            var r = new List<Models.VerificationMessage>();
            var manifests = System.IO.Directory.GetFiles(package.ExtractedPath, "*.dnn");
            if (manifests.Length == 1)
            {
                try
                {
                    var file = manifests[0];
                    using (var rdr = new System.Xml.XmlTextReader(new System.IO.FileStream(file, System.IO.FileMode.Open)))
                    {
                        while (rdr.Read()) { }
                    }
                }
                catch (Exception)
                {
                    r.Add(new Models.VerificationMessage { Message = "Package manifest must be properly formatted XML.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("7be70ad9-26e1-47c1-b7df-a53ed0c59c55"), Rule = GetType().ToString() });
                }
            }
            return r;
        }
    }
}