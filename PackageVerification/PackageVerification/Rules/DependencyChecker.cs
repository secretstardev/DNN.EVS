using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using PackageVerification.AssemblyScanner;
using PackageVerification.Models;

namespace PackageVerification.Rules
{
    public class DependencyChecker : IPackageRule
    {
        public List<VerificationMessage> ApplyRule(Package package)
        {
            var r = new List<VerificationMessage>();

            var coreVersionDependency = package.CoreVersionDependency;
            var minDNNVersion = package.MinDotNetNukeVersion;

            var coreVersionInt = Utilities.GetVersionSortOrder(coreVersionDependency);
            var minDNNVersionInt = Utilities.GetVersionSortOrder(minDNNVersion);

            if (coreVersionInt > minDNNVersionInt)
            {
                //warn that the manifest calls for a higer version then the assembilies reference.
                r.Add(new Models.VerificationMessage { Message = "The 'CoreVersion' dependency in the manifest is higher than the assembly references warrant.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("348907c9-485b-4a87-b4f1-e014b567ad5d"), Rule = GetType().ToString() });
            }
            else if (coreVersionInt < minDNNVersionInt)
            {
                //error that the assembilies reference a higher version of dnn then the dependency "CoreVersion" calls for.
                r.Add(new Models.VerificationMessage { Message = "The assemblies used in this extension, reference a higher version of DNN then the dependency “CoreVersion” in the manifest calls for.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("93b81bb0-fded-447a-a720-f117f73d40e1"), Rule = GetType().ToString() });
            }
            else if (coreVersionInt == minDNNVersionInt)
            {
                //they match do nothing.
            }

            return r;
        }
    }
}
