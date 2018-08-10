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