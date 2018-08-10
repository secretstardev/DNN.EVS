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
using System.IO;
using System.Xml;
using PackageVerification.SqlScanner;
using PackageVerification.SqlScanner.Models;
using PackageVerification.Models;
using System.Linq;

namespace PackageVerification.Rules.Manifest.Components
{
    public class ScriptNode : ComponentBase, IManifestRule
    {
        public List<VerificationMessage> ApplyRule(Package package, Models.Manifest manifest)
        {
            var r = new List<VerificationMessage>();

            try
            {
                if (manifest.ComponentNodes == null)
                    return r;

                foreach (XmlNode componentNode in manifest.ComponentNodes)
                {
                    if (componentNode.Attributes == null) continue;

                    var type = componentNode.Attributes["type"];
                    if (type == null || string.IsNullOrEmpty(type.Value)) continue;

                    if (type.Value != "Script") continue;

                    var primaryNode = componentNode.SelectSingleNode("scripts");
                    if (primaryNode == null) continue;

                    var basePathNode = primaryNode.SelectSingleNode("basePath");
                    if (basePathNode == null)
                    {
                        r.Add(new VerificationMessage { Message = "ALL scripts nodes should have a basePath specified.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("c45d2a61-ffdd-4f92-8710-5cb3dd25deff"), Rule = GetType().ToString() });
                    }

                    var xmlNodeList = primaryNode.SelectNodes("script");
                    if (xmlNodeList == null) continue;

                    foreach (XmlNode innerNode in xmlNodeList)
                    {
                        var sqlScriptFile = new SqlScriptFile();

                        if (innerNode.Attributes != null && innerNode.Attributes.Count > 0)
                        {
                            var scriptType = innerNode.Attributes["type"];
                            if (scriptType != null && !string.IsNullOrEmpty(scriptType.Value))
                            {
                                if (scriptType.Value == "UnInstall")
                                {
                                    sqlScriptFile.ScriptType = ScriptType.UnInstall;
                                    r.Add(new VerificationMessage { Message = "Uninstall scripts were detected in the manifest", MessageType = Models.MessageTypes.Info, MessageId = new Guid("5f99f13a-c9ea-4b11-935c-a5dba0a9f8f8"), Rule = GetType().ToString() });
                                }
                            }
                        }

                        var nameNode = innerNode.SelectSingleNode("name");
                        if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                        {
                            sqlScriptFile.Name = nameNode.InnerText;
                        }
                        else
                        {
                            r.Add(new VerificationMessage { Message = "Each script node must have a 'name' node with in it.", MessageType = Models.MessageTypes.Error, MessageId = new Guid("dc50cb9e-906a-4215-b839-a8d6d92270ab"), Rule = GetType().ToString() });
                        }

                        var pathNode = innerNode.SelectSingleNode("path");
                        if (pathNode != null && !string.IsNullOrEmpty(pathNode.InnerText))
                        {
                            if (pathNode.InnerText.StartsWith("\\"))
                            {
                                r.Add(new VerificationMessage { Message = "The value of the 'path' node should not start with a \\.", MessageType = Models.MessageTypes.Warning, MessageId = new Guid("414ca7da-18a8-4a08-a2c9-e8eacb863966"), Rule = GetType().ToString() });
                                sqlScriptFile.Path = pathNode.InnerText.TrimStart('\\');
                            }
                            else
                            {
                                sqlScriptFile.Path = pathNode.InnerText;
                            }
                        }

                        var versionNode = innerNode.SelectSingleNode("version");
                        if (versionNode != null && !string.IsNullOrEmpty(versionNode.InnerText))
                        {
                            sqlScriptFile.Version = versionNode.InnerText;
                        }

                        var sourceFileName = string.Empty;
                        var sourceFileNameNode = innerNode.SelectSingleNode("sourceFileName");
                        if (sourceFileNameNode != null && !string.IsNullOrEmpty(sourceFileNameNode.InnerText))
                        {
                            sourceFileName = sourceFileNameNode.InnerText;
                        }

                        sqlScriptFile.TempFilePath = FullFilePath(package, sourceFileName, sqlScriptFile.Path, sqlScriptFile.Name);

                        manifest.Files.Add(sqlScriptFile.TempFilePath.ToLower());

                        manifest.SQLScripts.Add(sqlScriptFile);
                    }

                    ProcessScripts(r, package, manifest);
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the SQL Azure scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.Scripts: " + exc.Message, MessageType = Models.MessageTypes.SystemError, MessageId = new Guid("d93c52c9-5b54-48f0-b7ea-2315329fd512"), Rule = GetType().ToString() });
            }

            return r;
        }

        public void ProcessScripts(List<VerificationMessage> r, Package package, Models.Manifest manifest)
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);
                var messageList = new List<Azure>();
                var azureScanner = new ScriptScanner();

                foreach (var sqlScriptFile in manifest.InstallScripts)
                {
                    if (!File.Exists(sqlScriptFile.TempFilePath) || sqlScriptFile.IsUnInstall) continue;

                    //we have script files, now let's see if they will work in Azure.
                    string fileName;

                    if (sqlScriptFile.TempFilePath.Contains("\\"))
                    {
                        var fileParts = sqlScriptFile.TempFilePath.Split('\\');
                        fileName = fileParts[fileParts.Length - 1];
                    }
                    else
                    {
                        fileName = sqlScriptFile.Name;
                    }

                    messageList.AddRange(azureScanner.ProcessAzure(sqlScriptFile.TempFilePath,
                        tempPath + "\\" + fileName));
                }

                //var outputFilePaths = Directory.GetFiles(tempPath, "*.SqlDataProvider");
                var outputFilePaths = Directory.GetFiles(tempPath, "*.sql");

                package.SQLOutputPath = tempPath;

                foreach (var filePath in outputFilePaths)
                {
                    messageList.AddRange(azureScanner.LogAzure(filePath));
                }

                foreach (var azure in messageList)
                {
                    var messageType = azure.MessageType.ToString();

                    if (messageType == Models.MessageTypes.Error.ToString())
                    {
                        r.Add(new VerificationMessage
                        {
                            Message = azure.ToString(),
                            MessageType = Models.MessageTypes.Error,
                            MessageId = new Guid("6f6ed7be-c182-4baf-a369-809b7d99549a"),
                            Rule = GetType().ToString()
                        });
                    }
                    else if (messageType == Models.MessageTypes.Info.ToString())
                    {
                        //r.Add(new Models.VerificationMessage { Message = azure.ToString(), MessageType = Models.MessageTypes.Info, MessageId = new Guid("1d4eba86-a23f-4bd0-afdf-430ed4c4a2a8"), Rule = GetType().ToString() });
                    }
                    else if (messageType == Models.MessageTypes.SystemError.ToString())
                    {
                        r.Add(new VerificationMessage
                        {
                            Message = azure.ToString(),
                            MessageType = Models.MessageTypes.SystemError,
                            MessageId = new Guid("a2cb34dd-c882-4302-ade0-d38c75e98fa2"),
                            Rule = GetType().ToString()
                        });
                    }
                    else if (messageType == Models.MessageTypes.Warning.ToString())
                    {
                        r.Add(new VerificationMessage
                        {
                            Message = azure.ToString(),
                            MessageType = Models.MessageTypes.Warning,
                            MessageId = new Guid("3a9452d3-dce5-4f78-93af-12645ae224ac"),
                            Rule = GetType().ToString()
                        });
                    }
                    else
                    {
                        r.Add(new VerificationMessage
                        {
                            Message = "Could not parse the SQL Azure error message type.",
                            MessageType = Models.MessageTypes.SystemError,
                            MessageId = new Guid("4fef4b2c-4b8d-4ff9-b345-072a53a84a35"),
                            Rule = GetType().ToString()
                        });
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the SQL Azure scanner", exc.Message);
                r.Add(new VerificationMessage
                {
                    Message = "An Error occurred while processing Rules.Manifest.Components.Scripts: " + exc.Message,
                    MessageType = Models.MessageTypes.SystemError,
                    MessageId = new Guid("6addcb0f-41c7-4b8f-b521-ca5f92d26285"),
                    Rule = GetType().ToString()
                });
            }
        }
    }
}