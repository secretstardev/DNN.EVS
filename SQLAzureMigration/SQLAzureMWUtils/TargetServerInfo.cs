using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    [Serializable]
    public class TargetServerInfo : IComparable
    {
        public bool Save { get; set; }
        public ServerTypes ServerType { get; set; }
        public string ServerInstance { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string RootDatabase { get; set; }
        public string TargetDatabase { get; set; }
        public bool LoginSecure { get; set; }
        public string Version { get; set; }

        public TargetServerInfo()
        {
            ServerInstance = "";
            Login = "";
            Password = "";
            RootDatabase = "";
            TargetDatabase = "";
            LoginSecure = false;
            Save = false;
        }

        public string ConnectionStringRootDatabase
        {
            get
            {
                return CommonFunc.GetConnectionString(ServerInstance, LoginSecure, RootDatabase, Login, Password);
            }
        }

        public string ConnectionStringTargetDatabase
        {
            get
            {
                return CommonFunc.GetConnectionString(ServerInstance, LoginSecure, TargetDatabase, Login, Password);
            }
        }
        
        public int CompareTo(object obj)
        {
            return ServerInstance.CompareTo(((TargetServerInfo)obj).ServerInstance);
        }
    }
}
