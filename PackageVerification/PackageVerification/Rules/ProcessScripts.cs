#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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