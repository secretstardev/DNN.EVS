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
