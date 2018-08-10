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
using System.Configuration;
using System.IO;
using System.Globalization;
using Microsoft.SqlServer.Management.Smo;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Management.Common;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlClient;
using System.Data;

namespace SQLAzureMWUtils
{
    /// <summary>
    /// General functions.
    /// </summary>
    /// <devdoc>
    /// This class is used for general functions that are used within SQLAzureMW
    /// </devdoc>
    /// <author name="George Huey" />
    /// <history>
    ///     <change date="9/9/2009" user="George Huey">
    ///         Cleaned up comments, added headers, etc.
    ///     </change>
    /// </history>
    public delegate void AsyncUpdateStatus(AsyncNotificationEventArgs e);
    public delegate void AsyncBCPJobUpdateStatus(AsyncBCPJobEventArgs e);
    public delegate void AsyncQueueBCPJob(AsyncQueueBCPJobArgs e);
    public delegate void AsyncUpdateTab(AsyncTabPageEventArgs e);

    public static class CommonFunc
    {
        public static string SerializeToXmlString(object obj)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            XmlSerializer ser = new XmlSerializer(obj.GetType());

            ser.Serialize(ms, obj);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            System.IO.StreamReader tr = new System.IO.StreamReader(ms);

            return tr.ReadToEnd();
        }

        public static object DeserializeXmlString(string xmlString, Type toTypeOf)
        {
            XmlSerializer mySerializer = new XmlSerializer(toTypeOf);
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);

            sw.Write(xmlString);
            sw.Flush();
            ms.Position = 0;
            return mySerializer.Deserialize(ms);
        }

        public static byte[] SerializeAndCompressToByteArray(object objX)
        {
            // First serialize the object into a memory stream
            MemoryStream stream = new MemoryStream();
            BinaryFormatter objBinaryFormat = new BinaryFormatter();
            objBinaryFormat.Serialize(stream, objX);

            // Convert the stream into a byte array
            stream.Position = 0;
            byte[] byteArray = stream.ToArray();

            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream sw = new GZipStream(ms, CompressionMode.Compress))
                {
                    sw.Write(byteArray, 0, byteArray.Length); // Compress
                }
                return ms.ToArray(); // Transform byte[] zip data to string
            }
        }

        public static T DecompressDeserializeByteArray<T>(byte[] btarr)
        {
            int dataLength = 0;

            // Prepare for decompress:  What we need to do is get the number of bytes to allocate for a decompressed
            // string.  So do this, we create a memory stream and run it through GZipStream.  Now note that from
            // what I can see, GZipStream closes the memory stream when it is done thus I can't do a ms.Seek or ms.Position
            // to start at the beginning of the stream.
            using (MemoryStream ms = new MemoryStream(btarr))
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                {
                    while (gz.ReadByte() > -1)
                    {
                        ++dataLength;
                    }
                }
            }

            byte[] byteArray = new byte[dataLength];

            // Now that we have the output byte array allocated, we need to start over (due to the note above)

            using (MemoryStream ms = new MemoryStream(btarr))
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                {
                    int rByte = gz.Read(byteArray, 0, dataLength);                               // Decompress
                    using (MemoryStream memoryStream = new MemoryStream(byteArray, 0, rByte))    // Convert the decompressed bytes into a stream
                    {
                        BinaryFormatter BinaryFormatter = new BinaryFormatter();                 // deserialize the stream into an object graph
                        return (T)BinaryFormatter.Deserialize(memoryStream);
                    }
                }
            }
        }

        public static void CompressObjectToFile(object objX, string fileName)
        {
            File.WriteAllBytes(fileName, SerializeAndCompressToByteArray(objX));
        }

        public static T DecompressObjectFromFile<T>(string fileName)
        {
            return CommonFunc.DecompressDeserializeByteArray<T>(File.ReadAllBytes(fileName));
        }

        public static string ConverByteArrayToString(SqlBinary sqlValue)
        {
            string value = "0x" + BitConverter.ToString((byte[])sqlValue).Replace("-", "");
            return value;
        }

        public static string GetLowestException(Exception ex)
        {
            Exception innerExcept = ex;
            while (innerExcept.InnerException != null)
            {
                innerExcept = innerExcept.InnerException;
            }
            return innerExcept.Message;
        }

        public static string FormatString(string str, params object[] args)
        {
            return string.Format(System.Globalization.CultureInfo.CurrentUICulture, str, args);
        }

        public static string GetTextFromFile(string fileToProcess)
        {
            StreamReader srFileToProcess = new StreamReader(fileToProcess, true);
            string txt = srFileToProcess.ReadToEnd();
            srFileToProcess.Close();
            return txt;
        }

        public static Boolean GetAppSettingsBoolValue(string settingName)
        {
            string settingValue = ConfigurationManager.AppSettings[settingName];
            if (settingValue != null)
            {
                if (settingValue.Equals(Properties.Resources.True, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }

                if (settingValue.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetAppSettingsStringValue(string settingName)
        {
            string settingValue = ConfigurationManager.AppSettings[settingName];
            if (settingValue != null)
            {
                return settingValue;
            }
            return "";
        }

        public static string GetConnectionString(Server targetServer, string targetDatabase)
        {
            return GetConnectionString(targetServer.ConnectionContext.ServerInstance, targetServer.ConnectionContext.LoginSecure, targetDatabase, targetServer.ConnectionContext.Login, targetServer.ConnectionContext.Password);
        }

        public static string GetConnectionString(string ServerInstance, bool LoginSecure, string TargetDatabase, string Login, string Password)
        {
            string connectionStr;

            if (LoginSecure)
            {
                connectionStr = FormatString(ConfigurationManager.AppSettings["ConnectionStringTrusted"], ServerInstance, TargetDatabase);
            }
            else
            {
                connectionStr = FormatString(ConfigurationManager.AppSettings["ConnectionString"], ServerInstance, TargetDatabase, Login, "'" + Password.Replace("'", "''") + "'");
            }
            return connectionStr;
        }

        public static bool IsDBOnline(TargetServerInfo targetServer)
        {
            bool dbOnline = false;
            string query = "select state_desc from sys.databases where name = '" + targetServer.TargetDatabase + "'";

            Retry.ExecuteRetryAction(() =>
            {
                using (SqlConnection connection = new SqlConnection(targetServer.ConnectionStringRootDatabase))
                {
                    ScalarResults sr = SqlHelper.ExecuteScalar(connection, CommandType.Text, query.ToString());
                    if (sr != null && sr.ExecuteScalarReturnValue != null)
                    {
                        string val = sr.ExecuteScalarReturnValue.ToString();
                        if (val != null && val.Equals("ONLINE"))
                        {
                            dbOnline = true;
                            return;
                        }
                    }
                    else
                    {
                        dbOnline = false;
                        return;
                    }
                }
            });
            return dbOnline;
        }

        public static string GetBcpOutputFileName(string directory, string tableName, int cnt, string ext)
        {
            string file = "";
            string slash = directory.EndsWith("\\", StringComparison.Ordinal) ? "" : "\\";

            tableName = tableName.Replace("/", "_SLASH_");

            if (cnt < 1)
            {
                file = directory + slash + tableName + "." + ext;
            }
            else
            {
                file = directory + slash + tableName + "_" + cnt.ToString(CultureInfo.InvariantCulture) + "." + ext;
            }

            if (!File.Exists(file))
            {
                return file;
            }

            if (CommonFunc.GetAppSettingsBoolValue("DelOldBCPFiles"))
            {
                File.Delete(file);
                return file;
            }

            return GetBcpOutputFileName(directory, tableName, ++cnt, ext);
        }

        public static Server GetSmoServer(ServerConnection connection)
        {
            Server dbServer = new Server(connection);

            dbServer.SetDefaultInitFields(typeof(Database), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(SqlAssembly), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(DatabaseDdlTrigger), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(Trigger), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(Schema), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(DatabaseRole), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(StoredProcedure), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(Table), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedDataType), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedFunction), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedTableType), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.View), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(XmlSchemaCollection), new String[] { "Name" });

            return dbServer;
        }

        public static Server GetSmoServer(string name)
        {
            Server dbServer = new Server(name);

            dbServer.SetDefaultInitFields(typeof(Database), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(SqlAssembly), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(DatabaseDdlTrigger), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(Trigger), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(Schema), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(DatabaseRole), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(StoredProcedure), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(Table), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedDataType), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedFunction), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedTableType), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.View), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(XmlSchemaCollection), new String[] { "Name" });

            return dbServer;
        }

        public static List<KeyValuePair<string, string>> GetTargetServerTypes()
        {
            List<KeyValuePair<string, string>> targetServers = new List<KeyValuePair<string, string>>();

            targetServers.Add(new KeyValuePair<string, string>("ServerType_AzureSQLDatabase", Properties.Resources.ServerType_AzureSQLDatabase));
            targetServers.Add(new KeyValuePair<string, string>("ServerType_AzureSQLDatabaseV12", Properties.Resources.ServerType_AzureSQLDatabaseV12));
            targetServers.Add(new KeyValuePair<string, string>("ServerType_SQLServer", Properties.Resources.ServerType_SQLServer));
            return targetServers;
        }

        public static ServerTypes GetEnumServerType(string serverType)
        {
            switch (serverType.ToLower())
            {
                case "servertype_azuresqldatabase":
                    return ServerTypes.AzureSQLDB;

                default:
                    return ServerTypes.SQLServer;
            }
        }

        public static string[] GetSchemaAndTableFromBCPArgs(string bcpArgs)
        {
            string[] arr = {"", ""};

            int start = bcpArgs.IndexOf("[", 0) + 1;
            if (start > 0)
            {
                int end = bcpArgs.IndexOf("].", start + 1);

                if (end > start)
                {
                    arr[0] = bcpArgs.Substring(start, end - start);

                    start = bcpArgs.IndexOf(".[", end) + 2;
                    if (start > 2)
                    {
                        end = bcpArgs.IndexOf("] ", start + 1);
                        if (end > start)
                        {
                            arr[1] = bcpArgs.Substring(start, end - start);
                            return arr;
                        }
                    }
                }
            }
            throw new Exception(Properties.Resources.ErrorParsingBCPArgs + " { " + bcpArgs + " }");
        }

        public static string BuildBCPUploadCommand(TargetServerInfo targetServer, string bcpArgs)
        {
            // bcpArgs example: ":128425:[dbo].[Categories] in "C:\Users\ghuey\AppData\Local\Temp\tmp1CD8.tmp" -n -E

            string[] schema_table = GetSchemaAndTableFromBCPArgs(bcpArgs);
            string schema = schema_table[0];
            string table = schema_table[1];

            StringBuilder args = new StringBuilder(200);

            if (bcpArgs.EndsWith("\r", StringComparison.Ordinal))
            {
                bcpArgs = bcpArgs.Remove(bcpArgs.Length - 1);
            }
            string filteredCommands = bcpArgs.Substring(bcpArgs.IndexOf(" in ", StringComparison.Ordinal));

            /*
            ** Note that BCP currently does not handle databases, schemas, and tables with "." in them very well.  Thus,
            ** this next section of code.  If there are no "." in any of these, then we can just "" the string and put the -q (quoted) flag
            ** on the BCP command and BCP will be happy.  If there is just a period in the database name, then we can split
            ** out the database name (using the -d argument) and then "" everything out with the -q flag.  If there is a "."
            ** in more than one of the three, then we will have to [ ] the string out and hope for the best (without the -q).
            **
            ** The problem is that if you [ ] the string out, then you can't use the -q flag and if your table has computed column
            ** with an index, then BCP requires the -q flag (which you can't do with [ ].  Anyway, it is kind of a catch 22 problem.
            */

            if (targetServer.TargetDatabase.IndexOf('.') < 1 && schema.IndexOf('.') < 1 && table.IndexOf('.') < 1)
            {
                args.Append("\"" + targetServer.TargetDatabase + "." + schema + "." + table + "\"" + filteredCommands + " -q -S " + targetServer.ServerInstance);
            }
            else if (targetServer.TargetDatabase.IndexOf('.') > 0 && schema.IndexOf('.') < 1 && table.IndexOf('.') < 1)
            {
                args.Append("\"" + schema + "." + table + "\"" + filteredCommands + " -d \"" + targetServer.TargetDatabase + "\" -q -S " + targetServer.ServerInstance);
            }
            else
            {
                args.Append("[" + targetServer.TargetDatabase + "].[" + schema + "].[" + table + "]" + filteredCommands + " -S " + targetServer.ServerInstance);
            }

            if (targetServer.LoginSecure)
            {
                args.Append(" -T");
            }
            else
            {
                string login = targetServer.Login[0] == '"' ? targetServer.Login : "\"" + targetServer.Login + "\"";
                string password = targetServer.Password[0] == '"' ? targetServer.Password : "\"" + targetServer.Password + "\"";
                args.Append(" -U " + login + " -P " + password);
            }
            return args.ToString();
        }
    }

    public class TemporaryFileStream : FileStream
    {
        private string _fileName = string.Empty;
        private bool _deleteFile = true;

        public TemporaryFileStream()
            : this(Path.GetTempFileName())
        {
        }

        public bool DeleteFile
        {
            get { return _deleteFile; }
            set { _deleteFile = value; }
        }

        public TemporaryFileStream(string fileName)
            : base(fileName, FileMode.OpenOrCreate)
        {
            _deleteFile = false;
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_deleteFile && FileName.Length > 1)
            {
                File.Delete(FileName);
            }
        }
    }
}
