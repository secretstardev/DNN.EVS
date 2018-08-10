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
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace PackageVerification.SQLRunner
{
    public class Common
    {
        internal static string BuildConnectionString(string databaseName, bool runAsSA = true)
        {
            string userName;
            string password;
            var serverAddress = System.Configuration.ConfigurationManager.AppSettings["TestDBServerAddress"];

            if (runAsSA)
            {
                userName = System.Configuration.ConfigurationManager.AppSettings["TestDBServerUserName"];
                password = System.Configuration.ConfigurationManager.AppSettings["TestDBServerPassword"];
            }
            else
            {
                userName = databaseName + "_admin";
                password = System.Configuration.ConfigurationManager.AppSettings["TestDBPassword"];
            }

            return "Server = " + serverAddress + "; Database = " + databaseName + "; User Id = " + userName + "; Password = " + password + ";";
        }

        internal static void RunSQLScriptViaSMO(string databaseName, string script, bool runAsSA = true, bool replaceTokens = false)
        {
            if (replaceTokens)
                script = ReplaceTokens(script);

            var retryCount = 0;

            var conn = new SqlConnection(BuildConnectionString(databaseName, runAsSA));
            var server = new Server(new ServerConnection(conn));

            Retry:

            try
            {
                server.ConnectionContext.ExecuteNonQuery(script);
            }
            catch (Exception ex)
            {
                if (retryCount < 5)
                {
                    retryCount++;
                    goto Retry;
                }

                throw ex;
            }
        }
        
        internal static void RunSQLScript(string databaseName, string script, bool runAsSA = true, bool replaceTokens = false)
        {
            if (replaceTokens)
                script = ReplaceTokens(script);

            Regex regex = new Regex("^\\s*GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(script);

            var connection = new SqlConnection { ConnectionString = BuildConnectionString(databaseName, runAsSA) };
            connection.Open();
            
            try
            {
                SqlTransaction transaction = connection.BeginTransaction();

                using (SqlCommand cmd = connection.CreateCommand())
                {
                    var retryCount = 0;
                    cmd.Connection = connection;
                    cmd.Transaction = transaction;

                    foreach (string line in lines)
                    {
                        if (line.Length > 0)
                        {
                            cmd.CommandText = line;
                            cmd.CommandType = CommandType.Text;

                            Retry:
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (SqlException ex)
                            {
                                if (retryCount < 5)
                                {
                                    retryCount++;
                                    goto Retry;
                                }

                                transaction.Rollback();
                                throw ex;
                            }
                        }
                    }
                }

                transaction.Commit();
            }
            catch (SqlException)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }

        }

        internal static void RunSQLStatement(string databaseName, string statement, bool runAsSA = true, bool replaceTokens = false)
        {
            if (replaceTokens)
                statement = ReplaceTokens(statement);

            var connection = new SqlConnection { ConnectionString = BuildConnectionString(databaseName, runAsSA) };
            connection.Open();
            
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = statement;
                cmd.CommandType = CommandType.Text;

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        internal static string ReplaceTokens(string input)
        {
            var output = input.Replace("{databaseOwner}", "[TestSchema].");
            output = output.Replace("{objectQualifier}", "TestQualifier_");

            return output;
        }

        public static string CleanError(string input)
        {
            var output = input;

            output = output.Replace(System.Configuration.ConfigurationManager.AppSettings["TestDBServerAddress"], "SeverAddress");
            output = output.Replace(System.Configuration.ConfigurationManager.AppSettings["TestDBServerUserName"], "UserName");
            output = output.Replace(System.Configuration.ConfigurationManager.AppSettings["TestDBServerPassword"], "Password");
            output = output.Replace(System.Configuration.ConfigurationManager.AppSettings["TestDBPassword"], "Password");

            return output;
        }

        public static ServerConnection BuildServerConnection(string databaseName)
        {
            return new ServerConnection
                {
                    ServerInstance = System.Configuration.ConfigurationManager.AppSettings["TestDBServerAddress"],
                    SqlExecutionModes = SqlExecutionModes.ExecuteSql,
                    ConnectTimeout = 15,
                    DatabaseName = databaseName,
                    LoginSecure = false,
                    Login = System.Configuration.ConfigurationManager.AppSettings["TestDBServerUserName"],
                    Password = System.Configuration.ConfigurationManager.AppSettings["TestDBServerPassword"]
                };
        }
    }
}
