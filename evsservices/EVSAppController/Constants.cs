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

namespace EVSAppController
{
    public static class Constants
    {
        /// <summary>
        /// Configuration section key containing connection string.
        /// </summary>
        public const string ConfigurationSectionKey = "CloudStorageConnectionString";

        /// <summary>
        /// This is the name of the database that this system uses.
        /// </summary>
        public const string DataBaseName = "evsdatabase";

        /// <summary>
        /// Container where to upload files
        /// </summary>
        public const string ContainerName = "uploads";

        /// <summary>
        /// Container where to upload files
        /// </summary>
        public const string ContainerNameScripts = "scripts";

        /// <summary>
        /// Number of bytes in a Kb.
        /// </summary>
        public const int BytesPerKb = 1024;

        /// <summary>
        /// key to setting in web.config file that holds the password to the SMTP server
        /// </summary>
        public const string SmtpPassword = "SMTP_Password";

        /// <summary>
        /// key to setting in web.config file that holds the username to the SMTP server
        /// </summary>
        public const string SmtpUsername = "SMTP_Username";

        /// <summary>
        /// key to setting in web.config file that holds the address to the SMTP server
        /// </summary>
        public const string SmtpServer = "SMTP_Server";
        
        /// <summary>
        /// key to setting in web.config file that holds the email address used as the 'from' address
        /// </summary>
        public const string EmailFrom = "Email_From";

        /// <summary>
        /// key to setting in web.config file that holds the email address used as the 'to' address
        /// </summary>
        public const string EmailTo = "Email_To";
    }
}