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
using PackageVerification.Models;


namespace PackageVerification.Rules
{
    public class TwoWayFileChecker : IManifestRule
    {
        public List<VerificationMessage> ApplyRule(Package package, Models.Manifest manifest)
        {
            var r = new List<VerificationMessage>();

            var extractedFiles = FileList(package.ExtractedPath);

            //first we must clone
            var filesDiff = new List<string>();
            var extractedDiff = new List<string>();

            foreach (var file in manifest.Files)
            {
                if (!extractedFiles.Contains(file))
                {
                    var newFile = (string)file.Clone();
                    filesDiff.Add(newFile);
                }
            }

            foreach (var file in extractedFiles)
            {
                if (!manifest.Files.Contains(file))
                {
                    var newFile = (string)file.Clone();
                    extractedDiff.Add(newFile);
                }
            }

            //These files are in the manifest, but not in the extracted folder.
           foreach (var file in filesDiff)
            {
                var shortFile = file.Replace(package.ExtractedPath.ToLower(), "");
                r.Add(new VerificationMessage { Message = "A file (" + shortFile + ") specified in the manifest is missing from the extension.", MessageType = MessageTypes.Error, MessageId = new Guid("9ed37b47-82da-4aed-b24f-93c6bf32e141"), Rule = GetType().ToString() });
            }

            //These files are in the extracted folder, but are not in the manifest.
            foreach (var file in extractedDiff)
            {
                var shortFile = file.Replace(package.ExtractedPath.ToLower(), "");

                //we need to filter out extra manifest files as we don't need to warn about these.
                if (!(shortFile.EndsWith(".dnn") || shortFile.EndsWith(".dnn5") || shortFile.EndsWith(".dnn6")))
                {
                    r.Add(new VerificationMessage { Message = "A file (" + shortFile + ") was found in the extension that was not included in manifest.", MessageType = MessageTypes.Warning, MessageId = new Guid("d339cd9b-4c82-473c-9627-5e5c1507d852"), Rule = GetType().ToString() });
                }
            }
            return r;
        }

        private static List<string> FileList(string rootFolder)
        {
            var folders = new List<string>();

            folders.Add(rootFolder);
            
            Traverse(folders, rootFolder);

            var files = new List<string>();

            foreach (var folder in folders)
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    files.Add(file.ToLower());
                }
            }

            return files;
        }

        private static void Traverse(List<string> folders, string rootFolder)
        {
            try
            {
                // Test for UnauthorizedAccessException
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, rootFolder).Demand();

                Array.ForEach(Directory.GetDirectories(rootFolder), (directory) =>
                {
                    folders.Add(directory);

                    Traverse(folders, directory);
                });
            }
            catch
            {
                // Ignore folder that we don't have access to
            }
        }
    }
}
