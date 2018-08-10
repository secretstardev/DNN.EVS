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
using System.Data;
using System.Data.SqlClient;
using PackageVerification.SQLRunner.Models;

namespace PackageVerification.SQLRunner
{
    public class Objects
    {
        public static IEnumerable<DatabaseObject> GetDatabaseObjects(string databaseName)
        {
            var outputList = new List<DatabaseObject>();

            using (var connection = new SqlConnection(Common.BuildConnectionString(databaseName)))
            {
                var queryString = "SELECT name AS [Name], object_id AS [ObjectId], schema_id AS [SchemaId], parent_object_id AS [ParentObjectId], type AS [Type], type_desc AS [TypeDesc], create_date AS [CreateDate], modify_date AS [ModifyDate], SCHEMA_NAME(schema_id) AS [SchemaName] from sys.objects ORDER BY ObjectId ASC";

                var command = new SqlCommand(queryString, connection);
                connection.Open();

                var reader = command.ExecuteReader();

                // Call Read before accessing data. 
                while (reader.Read())
                {
                    outputList.Add(ReadSingleRow(reader));
                }

                // Call Close when done reading.
                reader.Close();
            }

            return outputList;
        }

        public static List<string> CheckDatabaseObject(string databaseName, DatabaseObject databaseObject)
        {
            var output = new List<string>();

            using (var connection = new SqlConnection(Common.BuildConnectionString(databaseName)))
            {
                var queryString = String.Format("EXECUTE sys.sp_refreshsqlmodule N'{0}';", databaseObject.FullSafeName);

                var command = new SqlCommand(queryString, connection);
                try
                {
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    foreach (SqlError err in ex.Errors)
                    {
                        output.Add(err.Message);
                    }
                }
                finally
                {
                    command.Connection.Close();
                }
            }

            return output;
        }

        public static string GetSchemaName(string databaseName, int schemaId)
        {
            var output = "";

            using (var connection = new SqlConnection(Common.BuildConnectionString(databaseName)))
            {
                var queryString = String.Format("SELECT SCHEMA_NAME({0});", schemaId);

                var command = new SqlCommand(queryString, connection);
                connection.Open();

                var reader = command.ExecuteReader();

                // Call Read before accessing data. 
                while (reader.Read())
                {
                    output = (string)reader[0];
                }

                // Call Close when done reading.
                reader.Close();
            }

            return output;
        }

        private static DatabaseObject ReadSingleRow(IDataRecord record)
        {
            var output = new DatabaseObject
                {
                    Name = (string) record[0],
                    ObjectId = (int) record[1],
                    SchemaId = (int) record[2],
                    ParentObjectId = (int) record[3],
                    Type = (string) record[4],
                    TypeDesc = (string) record[5],
                    CreateDate = (DateTime) record[6],
                    ModifyDate = (DateTime) record[7],
                    SchemaName = (string) record[8]
                };

            return output;
        }
    }
}
