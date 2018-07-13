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
