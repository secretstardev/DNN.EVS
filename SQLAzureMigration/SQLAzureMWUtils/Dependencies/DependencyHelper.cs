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
using System.Text;

namespace SQLAzureMWUtils
{
    public static class DependencyHelper
    {
        private static List<Dependency> _dependencies = null;

        public static List<Dependency> Dependencies
        {
            get
            {
                if (_dependencies == null)
                {
                    try
                    {
                        string file = CommonFunc.GetAppSettingsStringValue("DependencyFile");
                        if (file.Length > 0)
                        {
                            string dependsOn = CommonFunc.GetTextFromFile(file);
                            if (dependsOn.Length > 0)
                            {
                                _dependencies = (List<Dependency>)CommonFunc.DeserializeXmlString(dependsOn, typeof(List<Dependency>));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(CommonFunc.FormatString(Properties.Resources.DependencyCheckerFileNotFound, ex.Message));
                    }

                    if (_dependencies == null)
                    {
                        _dependencies = new List<Dependency>();
                    }
                }
                return _dependencies;
            }
        }

        public static bool CheckDependencies(ref string message)
        {
            // Create an instance of dependent classes in a new AppDomain
            try
            {
                string rulesEngineConfigFile = CommonFunc.GetAppSettingsStringValue("NotSupportedByAzureSQLDb");
                if (rulesEngineConfigFile.Length > 0 && !File.Exists(rulesEngineConfigFile))
                {
                    message = CommonFunc.FormatString(Properties.Resources.ErrorFileNotFound, rulesEngineConfigFile);
                    return false;
                }

                AppDomain newAppDomain = AppDomain.CreateDomain("DependencyChecker");
                foreach (Dependency depOn in Dependencies)
                {
                    newAppDomain.CreateInstance(depOn.Assembly, depOn.Type);
                }
            }
            catch (Exception ex)
            {
                Assembly assem = Assembly.GetEntryAssembly();
                AssemblyName assemName = assem.GetName();

                message = CommonFunc.FormatString(Properties.Resources.ErrorMissingDependencies, assemName.Name, ex.Message);
                return false;
            }
            return true;
        }
    }
}
