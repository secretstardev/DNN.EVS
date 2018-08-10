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
