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