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
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using PackageVerification.AssemblyScanner.Data;
using PackageVerification.Data;

namespace PackageVerification.AssemblyScanner
{
    public class ProcessAssembly
    {
        public static FrameworkInfo GetMinDotNetFramework(List<Models.Assembly> assemblies)
        {
            var minFramework = new FrameworkInfo();

            foreach (var assembly in assemblies)
            {
                if (assembly.Framework.SortOrder > minFramework.SortOrder)
                {
                    minFramework = assembly.Framework;
                }
            }

            return minFramework;
        }

        public static DotNetNukeVersionInfo GetMinDotNetNukeVersion(List<Models.Assembly> assemblies, string assemblyInfoFile)
        {
            var minDotNetNukeVersion = new DotNetNukeVersionInfo();

            foreach (var assembly in assemblies)
            {
                if(assembly.References != null)
                {
                    foreach (var reference in assembly.References)
                    {
                        var packageContainsReference = false;

                        foreach (var assemblyRef in assemblies)
                        {
                            if (assemblyRef.Name == reference.Name && assemblyRef.Version == reference.Version)
                            {
                                packageContainsReference = true;
                            }
                        }

                        if (!packageContainsReference)
                        {
                            var dnnAssembliesColl = DotNetNukeAssemblies.GetDotNetNukeAssembliesCollection(assemblyInfoFile);
                            foreach (var dotNetNukeAssembliesInfo in dnnAssembliesColl)
                            {
                                foreach (var assembly1 in dotNetNukeAssembliesInfo.AssemblyList)
                                {
                                    var tempSortOrder = minDotNetNukeVersion.SortOrder();
                                    var newSortOrder = dotNetNukeAssembliesInfo.DNNVersion.SortOrder();

                                    if (assembly1.Name == reference.Name && assembly1.Version == reference.Version && newSortOrder > tempSortOrder)
                                    {
                                        var lowestVer = new DotNetNukeVersionInfo();
                                        lowestVer.Name = "99.99.99";

                                        foreach (var netNukeAssembliesInfo in dnnAssembliesColl)
                                        {
                                            foreach (var assembly2 in netNukeAssembliesInfo.AssemblyList)
                                            {
                                                if(assembly1.Name == assembly2.Name && assembly1.Version == assembly2.Version && lowestVer.SortOrder() > netNukeAssembliesInfo.DNNVersion.SortOrder())
                                                {
                                                    lowestVer = netNukeAssembliesInfo.DNNVersion;
                                                }
                                            }   
                                        }

                                        if (minDotNetNukeVersion.SortOrder() < lowestVer.SortOrder())
                                        {
                                            minDotNetNukeVersion = lowestVer;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return minDotNetNukeVersion;
        }
        
        public static List<Models.Assembly> ProcessDirectory(string folderPath)
        {
            var output = new List<Models.Assembly>();

            var assemblies = Directory.GetFiles(folderPath, "*.dll");
            
            foreach (var assembly in assemblies)
            {
                output.Add(ProcessSingleAssembly(assembly));
            }

            return output;
        }

        public static Models.Assembly ProcessSingleAssembly(string filePath)
        {
            var output = new Models.Assembly();
            
            AppDomainSetup appSetup = new AppDomainSetup();
            appSetup.ApplicationBase = Environment.CurrentDirectory; //@"C:\Projects\Store Trunk\trunk\PackageVerification\PackageVerification\PackageVerification.AssemblyScanner\bin\Debug";

            Evidence baseEvidence = AppDomain.CurrentDomain.Evidence;
            Evidence evidence = new Evidence(baseEvidence);

            var sandbox = AppDomain.CreateDomain("Sandbox", evidence, appSetup);

            try
            {
                sandbox.SetData("filePath", filePath);

                sandbox.DoCallBack(MyCallBack);
                var imageRuntimeVersion = (String)sandbox.GetData("imageRuntimeVersion");
                var assemblyName = (String)sandbox.GetData("assemblyName");
                var referencedAssemblies = (AssemblyName[])sandbox.GetData("referencedAssemblies");
                //populate the base assembly info
                output = ProcessAssemblyInfo(output, assemblyName);

                //Let's get the base framework info
                foreach (var frameworkInfo in DotNetVersions.FrameworkList())
                {
                    if (imageRuntimeVersion == frameworkInfo.CLRVersion)
                    {
                        output.Framework = frameworkInfo;
                        break;
                    }
                }

                //populate the referenced assemblies list.
                foreach (var an in referencedAssemblies)
                {
                    var assembly = new Models.Assembly();
                    output.References.Add(ProcessAssemblyInfo(assembly, an.FullName));
                }
            }
            finally
            {
                AppDomain.Unload(sandbox);
            }

            return output;
        }

        static public void MyCallBack()
        {
            var filePath = AppDomain.CurrentDomain.GetData("filePath");

            var a = Assembly.ReflectionOnlyLoadFrom(filePath.ToString());

            AppDomain.CurrentDomain.SetData("imageRuntimeVersion", a.ImageRuntimeVersion);
            AppDomain.CurrentDomain.SetData("assemblyName", a.GetName().ToString());
            AppDomain.CurrentDomain.SetData("referencedAssemblies", a.GetReferencedAssemblies());
        }
        
        private static Models.Assembly ProcessAssemblyInfo(Models.Assembly input, string assemblyInfo)
        {
            var output = input;

            var parts = assemblyInfo.Split(',').Select(x => x.Trim()).ToList();

            var name = "";
            var version = "";
            var culture = "";
            var token = "";
            
            foreach (var part in parts)
            {
                if(part.StartsWith("Version="))
                {
                    version = part.Substring(8);
                }else if(part.StartsWith("Culture="))
                {
                    culture = part.Substring(8);
                }else if(part.StartsWith("PublicKeyToken="))
                {
                    token = part.Substring(15);
                }else
                {
                    name = part;
                }
            }

            output.Name = name;
            output.Version = version;
            output.Culture = culture;
            output.PublicKeyToken = token;

            return output;
        }
    }
}
