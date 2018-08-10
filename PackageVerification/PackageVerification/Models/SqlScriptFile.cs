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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageVerification.Models
{
    public class SqlScriptFile
    {
        public SqlScriptFile()
        {
            Name = string.Empty;
            Path = string.Empty;
            Version = string.Empty;
            ScriptType = ScriptType.Install;
            TempFilePath = string.Empty;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public ScriptType ScriptType { get; set; }
        public string TempFilePath { get; set; }

        public bool IsInstall
        {
            get { return Name.ToLower().StartsWith("install."); }
        }

        public bool IsUnInstall
        {
            get { return Name.ToLower().StartsWith("uninstall."); }
        }

        public int Rank {
            get { 
                var output = 0;
                var nameArray = Name.Split('.');

                var place = 10000;

                foreach (var s in nameArray)
                {
                    int val;
                    if (int.TryParse(s, out val) && place >= 1)
                    {
                        output += val*place;
                        place = place/100;
                    }
                }

                return output;
            }
        }
    }

    public enum ScriptType
    {
        Install,
        UnInstall
    }
}
