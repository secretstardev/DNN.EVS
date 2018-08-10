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
using System.IO;
using System.Security.Permissions;
using ICSharpCode.SharpZipLib.Zip;
using PackageVerification.AssemblyScanner;
using PackageVerification.Models;
using PackageVerification.Rules;
using PackageVerification.Rules.Manifest;
using PackageVerification.Rules.Manifest.Components;
using System.Collections.Generic;


namespace PackageVerification
{
    public class VerificationController
    {
        /// <summary>
        /// Try to parse the zip file and apply our rules
        /// </summary>
        /// <param name="path">Path to a DNN Package ZIP File</param>
        /// <returns>No messages during parsing were found</returns>
        public static Package TryParse(string path) {
            var p = new Package() { Path = path };
            return TryParse(p);
        }

        /// <summary>
        /// Try to parse the zip file and apply our rules
        /// </summary>
        public static Package TryParse(Package package)
        {
            if (package.Messages == null) package.Messages = new List<VerificationMessage>();

            var fz = new FastZip();
            var shortpath = Environment.GetEnvironmentVariable("ShortRootPath") ?? Path.GetTempPath();

            var tempPath = Path.Combine(shortpath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            package.ExtractedPath = tempPath;
            using (var stm = new FileStream(package.Path, FileMode.Open))
            {
                fz.ExtractZip(stm, tempPath, FastZip.Overwrite.Always, null, null, null, true, true);
            }

            package.Messages.AddRange(new PackageExists().ApplyRule(package));
            package.Messages.AddRange(new MinPackageSize().ApplyRule(package));
            package.Messages.AddRange(new ManifestFileExists().ApplyRule(package));
            package.Messages.AddRange(new ManifestIsXML().ApplyRule(package));

            foreach (var manifest in package.DNNManifests)
            {
                package.Messages.AddRange(new CorrectTypeAndVersion().ApplyRule(package, manifest));

                switch (manifest.ManifestType)
                {
                    case ManifestTypes.Package:
                        package.Messages.AddRange(new PackageNodeAndChildren().ApplyRule(package, manifest));
                        package.Messages.AddRange(new PackageOwner().ApplyRule(package, manifest));
                        package.Messages.AddRange(new AssemblyNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new AuthenticationSystemNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new DashboardControlNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new CleanupNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new ConfigNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new FileNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new ModuleNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new SkinNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new SkinObjectNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new ContainerNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new CoreLanguageNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new ExtensionLanguageNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new ResourceFileNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new ScriptNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new WidgetNode().ApplyRule(package, manifest));
                        break;
                    case ManifestTypes.Module:
                        package.Messages.AddRange(new FolderNodeAndChildren().ApplyRule(package, manifest));
                        package.Messages.AddRange(new FileNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new ModuleNode().ApplyRule(package, manifest));
                        package.Messages.AddRange(new ProcessScripts().ApplyRule(package, manifest));
                        break;
                }
                package.Messages.AddRange(new TwoWayFileChecker().ApplyRule(package, manifest));

                var assemblyInfoFile = @"..\..\..\DNNPackages\collection.xml";
                package.MinDotNetVersion = ProcessAssembly.GetMinDotNetFramework(package.Assemblies).VersionName;
                package.MinDotNetNukeVersion = ProcessAssembly.GetMinDotNetNukeVersion(package.Assemblies, assemblyInfoFile).Name;

                package.Messages.AddRange(new SQLTestRunner().ApplyRule(package, manifest));
            }

            UnZipAll(package.ExtractedPath);

            package.Messages.AddRange(new FileEncodingChecker().ApplyRule(package));
            package.Messages.AddRange(new UnsafeFileChecker().ApplyRule(package));
            package.Messages.AddRange(new DependencyChecker().ApplyRule(package));

            foreach (VerificationMessage msg in package.Messages)
            {
                if (msg.MessageType == MessageTypes.Info)
                {
                    package.HasInfos = true;
                }
                if (msg.MessageType == MessageTypes.Error)
                {
                    package.HasErrors = true;
                }
                if (msg.MessageType == MessageTypes.Warning)
                {
                    package.HasWarnings = true;
                }
                //if (package.HasErrors) break;
            }

            return package;
        }

        private static void UnZipAll(string rootFolder)
        {
            var folders = new List<string>();

            folders.Add(rootFolder);

            Traverse(folders, rootFolder);

            foreach (var folder in folders)
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    if (file.EndsWith(".zip"))
                    {
                        var fz = new FastZip();
                        var tempPath = Path.Combine(folder, Guid.NewGuid().ToString());
                        Directory.CreateDirectory(tempPath);
                        using (var stm = new FileStream(file, FileMode.Open))
                        {
                            fz.ExtractZip(stm, tempPath, FastZip.Overwrite.Always, null, null, null, true, true);
                        }
                    }
                }
            }
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