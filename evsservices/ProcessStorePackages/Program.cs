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
using System.Data.SqlTypes;
using System.IO;
using System.Transactions;
using EVSAppController;
using System.Configuration;
using EVSAppController.Controllers;
using EVSAppController.Models;
using Microsoft.WindowsAzure.Storage;

namespace ProcessStorePackages
{
    class FileObject
    {
        public int ItemId { get; set; }
        public string FileId { get; set; }
        public string ContentId { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string Uri { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to begin.");
            Console.ReadKey();

            Console.WriteLine("Starting...");

            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine("Looking for a file to process");
                var file = MoveAFile();

                while (file == null)
                {
                    System.Threading.Thread.Sleep(60000);
                    file = MoveAFile();
                }
            
                Console.WriteLine("File moved up to Azure storage.");

                AddFileToQueue(file);

                //next we need to check the status up the item
                var status = StatusController.GetCurrentStatus(file.FileId, "61e8b5db-d076-44c3-914d-05e37cef4abd");
            
                while (status.Finished == false)
                {
                    System.Threading.Thread.Sleep(10000);
                    status = StatusController.GetCurrentStatus(file.FileId, "61e8b5db-d076-44c3-914d-05e37cef4abd");
                }

                Console.WriteLine("File done processing, obtaining results.");
                
                var extensionOutput = ExtensionController.GetExtension(file.FileId);

                //next step is to save the results in the database.
                ProcessResult(extensionOutput, file);

                Console.WriteLine("Results done processing, Time to sleep.");

                System.Threading.Thread.Sleep(60000);
            }

            Console.WriteLine("Done...");
            Console.ReadKey();
        }

        private static FileObject MoveAFile()
        {
            var output = new FileObject();

            using (var transactionScope = new TransactionScope())
            {
                var connString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

                using (var sqlConnection = new SqlConnection(connString))
                {
                    sqlConnection.Open();

                    int itemId;
                    string contentGUID;
                    string filePath;
                    byte[] transactionContext;

                    Console.WriteLine("Obtaining next file to process.");

                    using (var sqlCommand = new SqlCommand("[dbo].[Store_Downloads_EVS_GetNextItemToProcess]", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        using (var reader = sqlCommand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                itemId = (int)reader["ItemID"];
                                contentGUID = reader["ContentGUID"].ToString();
                                filePath = (string)reader["Path"];
                                transactionContext = (byte[])reader["TransactionContext"];
                                reader.Close();
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }

                    Console.WriteLine("Found a file, reading stream.");

                    using (var source = new SqlFileStream(filePath, transactionContext, FileAccess.Read))
                    {
                        var newFileName = contentGUID + ".dat";
                        var connectionString = ConfigurationManager.AppSettings[Constants.ConfigurationSectionKey];
                        var storageAccount = CloudStorageAccount.Parse(connectionString);
                        var container = storageAccount.CreateCloudBlobClient().GetContainerReference(Constants.ContainerName);
                        container.CreateIfNotExists();

                        var blob = container.GetBlockBlobReference(newFileName);
                        var fileSize = (int)source.Length;
                        var buffer = new byte[fileSize];
                        source.Read(buffer, 0, buffer.Length);
                        source.Close();

                        using (var stream = new MemoryStream(buffer, false))
                        {
                            blob.UploadFromStream(stream);
                        }

                        output.ItemId = itemId;
                        output.FileId = Guid.NewGuid().ToString();
                        output.ContentId = contentGUID;
                        output.FileName = contentGUID + ".zip";
                        output.FileSize = fileSize;
                        output.Uri = blob.Uri.ToString();
                    }
                }

                transactionScope.Complete();
            }

            return output;
        }

        private static void AddFileToQueue(FileObject fileObject)
        {
            var file = new UploadQueue
            {
                FileID = fileObject.FileId,
                OriginalFileName = fileObject.FileName,
                FileLocation = fileObject.Uri,
                ContentType = "application/zip", //TODO: I should try and actually figure out what the ContentType is.
                ContentSize = fileObject.FileSize,
                UserKey = "61e8b5db-d076-44c3-914d-05e37cef4abd",
                UserIP = "0.0.0.0",
                UploadedDate = DateTime.Now,
                Processing = false
            };

            UploadQueueController.AddItemToQueue(file);
        }

        private static void ProcessResult(Extension result, FileObject fileObject)
        {            
            var connString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

            using (var sqlConnection = new SqlConnection(connString))
            {
                sqlConnection.Open();
                try
                {
                    //we need to store the ExtensionDetail items
                    foreach (var extensionDetails in result.ExtensionDetails)
                    {
                        using (var sqlCommand = new SqlCommand("[dbo].[Store_Downloads_EVS_ExtensionDetail_Insert]", sqlConnection))
                        {
                            sqlCommand.CommandType = CommandType.StoredProcedure;

                            sqlCommand.Parameters.Add(new SqlParameter("ExtensionDetailID", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Input,
                                Value = extensionDetails.ExtensionDetailID
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("ItemID", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Input,
                                Value = fileObject.ItemId
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("DetailID", SqlDbType.NChar)
                            {
                                Size = 36,
                                Direction = ParameterDirection.Input,
                                Value = extensionDetails.DetailID
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("DetailName", SqlDbType.NVarChar)
                            {
                                Size = 250,
                                Direction = ParameterDirection.Input,
                                Value = extensionDetails.DetailName
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("DetailValue", SqlDbType.NVarChar)
                            {
                                Size = 250,
                                Direction = ParameterDirection.Input,
                                Value = extensionDetails.DetailValue
                            });

                            sqlCommand.ExecuteNonQuery();
                        }
                    }

                    //then we need to store the ExtensionMessage items
                    foreach (var extensionMessages in result.ExtensionMessages)
                    {
                        using (var sqlCommand = new SqlCommand("[dbo].[Store_Downloads_EVS_ExtensionMessage_Insert]", sqlConnection))
                        {
                            sqlCommand.CommandType = CommandType.StoredProcedure;

                            sqlCommand.Parameters.Add(new SqlParameter("ExtensionMessageID", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Input,
                                Value = extensionMessages.ExtensionMessageID
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("ItemID", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Input,
                                Value = fileObject.ItemId
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("MessageTypeID", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Input,
                                Value = extensionMessages.MessageTypeID
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("MessageID", SqlDbType.NChar)
                            {
                                Size = 36,
                                Direction = ParameterDirection.Input,
                                Value = extensionMessages.MessageID
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("Message", SqlDbType.NVarChar)
                            {
                                Size = -1,
                                Direction = ParameterDirection.Input,
                                Value = extensionMessages.Message
                            });

                            sqlCommand.Parameters.Add(new SqlParameter("Rule", SqlDbType.NVarChar)
                            {
                                Size = 250,
                                Direction = ParameterDirection.Input,
                                Value = extensionMessages.Rule
                            });

                            sqlCommand.ExecuteNonQuery();
                        }
                    }

                    //then we need to update the Store_Downloads_EVS_ScanLog table.
                    using (var sqlCommand = new SqlCommand("[dbo].[Store_Downloads_EVS_ScanLog_Insert]", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        sqlCommand.Parameters.Add(new SqlParameter("ItemID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Input,
                            Value = fileObject.ItemId
                        });

                        sqlCommand.Parameters.Add(new SqlParameter("LastScan", SqlDbType.DateTime)
                        {
                            Direction = ParameterDirection.Input,
                            Value = DateTime.Now
                        });

                        sqlCommand.ExecuteNonQuery();
                    }
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
        }
    }
}