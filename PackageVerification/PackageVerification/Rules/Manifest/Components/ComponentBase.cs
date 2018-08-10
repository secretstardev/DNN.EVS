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
using System.Xml;
using PackageVerification.Models;

namespace PackageVerification.Rules.Manifest.Components
{
    public class ComponentBase : ManifestRulesBase
    {
        protected void ProcessComponentNode(List<VerificationMessage> r, Package package, Models.Manifest manifest, XmlNode node, string innerNodeName)
        {
            var xmlNodeList = node.SelectNodes(innerNodeName);
            if (xmlNodeList != null)
            {
                foreach (XmlNode innerNode in xmlNodeList)
                {
                    ProcessNode(r, package, manifest, innerNode);
                }
            }
            else
            {
                r.Add(new VerificationMessage { Message = "Each " + node.Name + " node must have a at least one " + innerNodeName + " node within it.", MessageType = MessageTypes.Error, MessageId = new Guid("cdd09bca-9dfb-4658-9a5e-fa792c541ffd"), Rule = GetType().ToString() });
            }
        }

        protected string ProcessNode(List<VerificationMessage> r, Package package, Models.Manifest manifest, XmlNode node)
        {
            var path = string.Empty;
            var name = string.Empty;
            var sourceFileName = string.Empty;

            var pathNode = node.SelectSingleNode("path");

            if (pathNode != null && !string.IsNullOrEmpty(pathNode.InnerText))
            {
                if (pathNode.InnerText.StartsWith("\\"))
                {
                    r.Add(new VerificationMessage { Message = "The value of the 'path' node should not start with a \\.", MessageType = MessageTypes.Warning, MessageId = new Guid("414ca7da-18a8-4a08-a2c9-e8eacb863966"), Rule = GetType().ToString() });
                    path = pathNode.InnerText.TrimStart('\\');
                }
                else
                {
                    path = pathNode.InnerText;
                }
            }

            var nameNode = node.SelectSingleNode("name");
            if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
            {
                name = nameNode.InnerText;
            }
            else
            {
                r.Add(new VerificationMessage { Message = "Each " + node.Name + " node must have a 'name' node with in it.", MessageType = MessageTypes.Error, MessageId = new Guid("123ceb51-45d8-4932-a7a5-c4032134c454"), Rule = GetType().ToString() });
            }

            var sourceFileNameNode = node.SelectSingleNode("sourceFileName");
            if (sourceFileNameNode != null && !string.IsNullOrEmpty(sourceFileNameNode.InnerText))
            {
                sourceFileName = sourceFileNameNode.InnerText;
            }

            var fullFilePath = FullFilePath(package, sourceFileName, path, name);

            manifest.Files.Add(fullFilePath.ToLower());

            if (manifest.ManifestType == ManifestTypes.Module && name.ToLower().EndsWith(".sqldataprovider"))
            {
                var sqlScriptFile = new SqlScriptFile();

                if (name.ToLower().Contains("uninstall"))
                {
                    sqlScriptFile.ScriptType = ScriptType.UnInstall;
                    r.Add(new VerificationMessage { Message = "Uninstall scripts were detected in the manifest", MessageType = Models.MessageTypes.Info, MessageId = new Guid("5f99f13a-c9ea-4b11-935c-a5dba0a9f8f8"), Rule = GetType().ToString() });
                }
                
                sqlScriptFile.Name = name;
                sqlScriptFile.Path = path;
                sqlScriptFile.TempFilePath = fullFilePath;

                manifest.SQLScripts.Add(sqlScriptFile);
            }

            return fullFilePath;
        }

        protected static string FullFilePath(Package package, string sourceFileName, string path, string name)
        {
            string fullFilePath;

            if (!String.IsNullOrEmpty(sourceFileName))
            {
                fullFilePath = sourceFileName;
            }
            else
            {
                if (!String.IsNullOrEmpty(path))
                {
                    fullFilePath = System.IO.Path.Combine(path, name);
                }
                else
                {
                    fullFilePath = name;
                }
            }

            fullFilePath = System.IO.Path.Combine(package.ExtractedPath, fullFilePath);
            return fullFilePath;
        }
    }
}
