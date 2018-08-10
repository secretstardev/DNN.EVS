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

namespace PackageVerification.Rules
{
    public class ManifestFileExists : Models.IPackageRule
    {
        public List<Models.VerificationMessage> ApplyRule(Models.Package package)
        {
            var r = new List<Models.VerificationMessage>();
            var manifests = Directory.GetFiles(package.ExtractedPath, "*.dnn");

            if (manifests.Length > 1)
            {
                r.Add(new Models.VerificationMessage { Message = "Package contains multiple manifest (.dnn, .dnn5, .dnn6, etc.) files.", MessageType = Models.MessageTypes.Info, MessageId = new Guid("d7ab0418-c623-4c00-8d8a-bdda72b72322"), Rule = GetType().ToString() });
            }

            if (manifests.Length < 1)
            {
                r.Add(new Models.VerificationMessage { Message = "Package must contain at least one manifest (.dnn, .dnn5, .dnn6, etc.) file for installation.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("52416eb4-1001-4208-a627-0b2ced7b7212"), Rule = GetType().ToString() });
            }

            var dnnManifests = new List<string>();
            var dnn5Manifests = new List<string>();
            var dnn6Manifests = new List<string>();

            foreach (var manifest in manifests)
            {
                if(manifest.EndsWith(".dnn"))
                {
                    dnnManifests.Add(manifest);
                }
                else if (manifest.EndsWith(".dnn5"))
                {
                    dnn5Manifests.Add(manifest);
                }
                else if (manifest.EndsWith(".dnn6"))
                {
                    dnn6Manifests.Add(manifest);
                }
            }

            if(dnnManifests.ToArray().Length == 1)
            {
                r.Add(new Models.VerificationMessage { Message = "Package contains a .dnn manifest file.", MessageType = Models.MessageTypes.Info, MessageId = new Guid("56129123-596b-4f1c-9e7e-fe03e781a224"), Rule = GetType().ToString() });
            } else if (dnnManifests.ToArray().Length > 1)
            {
                r.Add(new Models.VerificationMessage { Message = "Package should not contain more than one .dnn manifest file.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("13b9eec9-9029-4642-868e-8d84355686cd"), Rule = GetType().ToString() });
            }

            if (dnn5Manifests.ToArray().Length == 1)
            {
                r.Add(new Models.VerificationMessage { Message = "Package contains a .dnn5 manifest file.", MessageType = Models.MessageTypes.Info, MessageId = new Guid("51c8699a-8e99-4d0c-880e-9bcc00125786"), Rule = GetType().ToString() });
            }
            else if (dnn5Manifests.ToArray().Length > 1)
            {
                r.Add(new Models.VerificationMessage { Message = "Package should not contain more than one .dnn5 manifest file.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("7a9158ad-0a09-4df3-bfc7-509f3f906072"), Rule = GetType().ToString() });
            }

            if (dnn6Manifests.ToArray().Length == 1)
            {
                r.Add(new Models.VerificationMessage { Message = "Package contains a .dnn6 manifest file.", MessageType = Models.MessageTypes.Info, MessageId = new Guid("ae26ae03-7b40-4252-8aee-a5a170565231"), Rule = GetType().ToString() });
            }
            else if (dnn6Manifests.ToArray().Length > 1)
            {
                r.Add(new Models.VerificationMessage { Message = "Package should not contain more than one .dnn6 manifest file.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("9a047962-5490-48ad-9421-ddd69c05e3a0"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}