using System;

namespace PackageVerification.SqlScanner.Models
{
    [Serializable]
    public class Azure
    {
        public string FileName { get; set; }
        public MessageTypes MessageType { get; set; }
        public string Message { get; set; }

        public Azure()
        {
            FileName = "";
            MessageType = MessageTypes.SystemError;
            Message = "This is an uninitiated error.";
        }

        public Azure(string fileName, MessageTypes messageType, string messege)
        {
            FileName = fileName;
            MessageType = messageType;
            Message = messege;
        }

        public override string ToString()
        {
            var fileName = "";

            if (FileName.Contains("\\"))
            {
                var fileParts = FileName.Split('\\');
                fileName = fileParts[fileParts.Length - 1];
            }
            else
            {
                fileName = FileName;
            }

            return string.Format("({1} in {2}) - {0}", Message, MessageType.ToString(), fileName);
        }
    }

    [Serializable]
    public class AzureMigrationResults
    {
        public MessageTypes MessageType { get; set; }
        public string Message { get; set; }

        public AzureMigrationResults()
        {
            MessageType = MessageTypes.SystemError;
            Message = "This is an uninitiated error.";
        }

        public AzureMigrationResults(string fileName, MessageTypes messageType, string messege)
        {
            MessageType = messageType;
            Message = messege;
        }

        public override string ToString()
        {
            return string.Format("({1}) - {0}", Message, MessageType.ToString());
        }
    }
}
