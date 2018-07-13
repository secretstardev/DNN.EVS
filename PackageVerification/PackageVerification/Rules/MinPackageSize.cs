using System;
using System.Collections.Generic;

namespace PackageVerification.Rules
{
    public class MinPackageSize : Models.IPackageRule
    {
        public List<Models.VerificationMessage> ApplyRule(Models.Package package)
        {
            var r = new List<Models.VerificationMessage>();
            var fi = new System.IO.FileInfo(package.Path);
            var valid = (fi.Length >= 3);
            if (!valid)
            {
                r.Add(new Models.VerificationMessage { Message = "Package does not meet the size requirement.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("fe54f8ee-7dea-4f28-b009-14b900222410"), Rule = GetType().ToString() });
            }

            //less than 1KB
            if (fi.Length <= 1024)
            {
                r.Add(new Models.VerificationMessage { Message = "Package size might be a bit too small to be valid.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("18e0a0bf-adaf-4329-9cd6-2a71788ca5c6"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}