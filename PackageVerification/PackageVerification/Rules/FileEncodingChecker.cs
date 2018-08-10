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
    public class FileEncodingChecker : IPackageRule
    {
        public List<VerificationMessage> ApplyRule(Package package)
        {
            var r = new List<VerificationMessage>();
            
            //UnZipAll(package.ExtractedPath);

            var files = Directory.GetFiles(package.ExtractedPath, "*.resx", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var enc = GetFileEncoding(file);
                if (enc == null)
                {
                    r.Add(new VerificationMessage { Message = "Unknown encoding detected in the following file: " + file, MessageType = MessageTypes.Warning, MessageId = new Guid("50b2af7c-db25-4eeb-9acb-47936be97197"), Rule = GetType().ToString() });
                }
                else
                {
                    if (!enc.Equals(Encoding.Unicode))
                    {
                        r.Add(new VerificationMessage { Message = "incompatible encoding (" + enc + ") detected in the following file: " + file, MessageType = MessageTypes.Warning, MessageId = new Guid("1b9b2b2c-c42f-408c-8810-268d4a4f23fc"), Rule = GetType().ToString() });
                    }
                }
            }
            
            return r;
        }

        private static Encoding GetFileEncoding(string filePath)
        {
            Encoding enc;

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (file.CanSeek)
                {
                    var bom = new byte[4];

                    file.Read(bom, 0, 4);

                    if ((bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) || (bom[0] == 0xff && bom[1] == 0xfe) || (bom[0] == 0xfe && bom[1] == 0xff) || (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff))
                    {
                        enc = Encoding.Unicode;
                    }
                    else
                    {
                        enc = Encoding.ASCII;
                    }

                    file.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    enc = Encoding.ASCII;
                }

                file.Close();
            }

            return enc;
        }

        //private static void UnZipAll(string rootFolder)
        //{
        //    var folders = new List<string>();

        //    folders.Add(rootFolder);

        //    Traverse(folders, rootFolder);
            
        //    foreach (var folder in folders)
        //    {
        //        foreach (var file in Directory.GetFiles(folder))
        //        {
        //            if (file.EndsWith(".zip"))
        //            {
        //                var fz = new FastZip();
        //                var tempPath = Path.Combine(folder, Guid.NewGuid().ToString());
        //                Directory.CreateDirectory(tempPath);
        //                using (var stm = new FileStream(file, FileMode.Open))
        //                {
        //                    fz.ExtractZip(stm, tempPath, FastZip.Overwrite.Always, null, null, null, true, true);
        //                }
        //            }
        //        }
        //    }
        //}

        //private static void Traverse(List<string> folders, string rootFolder)
        //{
        //    try
        //    {
        //        // Test for UnauthorizedAccessException
        //        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, rootFolder).Demand();

        //        Array.ForEach(Directory.GetDirectories(rootFolder), (directory) =>
        //        {
        //            folders.Add(directory);

        //            Traverse(folders, directory);
        //        });
        //    }
        //    catch
        //    {
        //        // Ignore folder that we don't have access to
        //    }
        //}
    }
}
