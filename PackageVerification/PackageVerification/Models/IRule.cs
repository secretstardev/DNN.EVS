using System.Collections.Generic;

namespace PackageVerification.Models
{
    public interface IPackageRule
    {
        List<VerificationMessage> ApplyRule(Package package);
    }

    public interface IManifestRule
    {
        List<VerificationMessage> ApplyRule(Package package, Manifest manifest);
    }
}
