using System;

namespace PackageVerification.SQLRunner.Models
{
    public class Database
    {
        public string DatabaseName { get; set; }
        public string DataFileName { get; set; }
        public string DataPathName { get; set; }
        public string DataFileGrowth { get; set; }
        public string LogFileName { get; set; }
        public string LogPathName { get; set; }
        public string LogFilePath { get; set; }
        public string LogFileGrowth { get; set; }

        public string CreateDBQueryLong()
        {
            return " CREATE DATABASE " + DatabaseName + " ON PRIMARY "
                                      + " (NAME = " + DataFileName + ", "
                                      + " FILENAME = '" + DataPathName + "', "
                                      + " SIZE = 2MB,"
                                      + "	FILEGROWTH =" + DataFileGrowth + ") "
                                      + " LOG ON (NAME =" + LogFileName + ", "
                                      + " FILENAME = '" + LogPathName + "', "
                                      + " SIZE = 1MB, "
                                      + "	FILEGROWTH =" + LogFileGrowth + ") ";
        }

        public string CreateDBQueryShort()
        {
            return String.Format(@"CREATE DATABASE [{0}];", DatabaseName);
        }
    }
}
