using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using PackageVerification.Models;


namespace PackageVerification.Rules
{
    public class UnsafeFileChecker : IPackageRule
    {
        public List<VerificationMessage> ApplyRule(Package package)
        {
            var r = new List<VerificationMessage>();
            
            var aspxFiles = Directory.GetFiles(package.ExtractedPath, "*.aspx", SearchOption.AllDirectories);

            foreach (var file in aspxFiles)
            {                
                r.Add(new VerificationMessage { Message = "Potentially unsafe aspx file detected: " + file, MessageType = MessageTypes.Warning, MessageId = new Guid("a3260fc2-c826-4d31-9a21-9455c19c7293"), Rule = GetType().ToString() });                
            }
            
            var ashxFiles = Directory.GetFiles(package.ExtractedPath, "*.ashx", SearchOption.AllDirectories);

            foreach (var file in ashxFiles)
            {
                r.Add(new VerificationMessage { Message = "Potentially unsafe ashx file detected: " + file, MessageType = MessageTypes.Warning, MessageId = new Guid("f5398d9d-4133-4975-b19a-6e24a5d8cf43"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}
