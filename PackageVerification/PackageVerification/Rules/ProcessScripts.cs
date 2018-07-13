using System;
using System.Collections.Generic;
using System.Diagnostics;
using PackageVerification.Models;
using PackageVerification.Rules.Manifest.Components;

namespace PackageVerification.Rules
{
    public class ProcessScripts : IManifestRule
    {
        public List<VerificationMessage> ApplyRule(Package package, Models.Manifest manifest)
        {
            var r = new List<VerificationMessage>();
            try
            {
                switch (manifest.ManifestType)
                {
                    case ManifestTypes.Package:
                        break;
                    case ManifestTypes.Module:
                        var scriptNode = new ScriptNode();
                        scriptNode.ProcessScripts(r, package, manifest);
                        break;
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the process scripts scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing RulesProcessScripts", MessageType = MessageTypes.SystemError, MessageId = new Guid("3a8f7655-2806-48c5-adc2-46402614c991"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}