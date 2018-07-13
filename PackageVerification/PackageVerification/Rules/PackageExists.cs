using System;
using System.Collections.Generic;

namespace PackageVerification.Rules
{
    public class PackageExists : Models.IPackageRule
    {
        public List<Models.VerificationMessage> ApplyRule(Models.Package Package)
        {
            var r = new List<Models.VerificationMessage>();
            if (!System.IO.File.Exists(Package.Path))
            {
                r.Add(new Models.VerificationMessage { Message = "Package does not exist", MessageType = Models.MessageTypes.Error, MessageId = new Guid("12a4c191-e443-4417-988e-93a14a419423"), Rule = GetType().ToString() });
            }
            return r;
        }
    }
}
