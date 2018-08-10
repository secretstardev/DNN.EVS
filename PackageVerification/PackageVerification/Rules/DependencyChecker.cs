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
