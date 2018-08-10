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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EVSAppController.Controllers;
using EVSAppController.Models;
using ExtensionValidationService.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using EVSAppController;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace ExtensionValidationService.Controllers
{
    public class AsyncUploadBlockHandler : MultipartFileStreamProvider
    {
        private string _userId;
        private string _fileId;
        private int _blockId;
        public ReturnValue ReturnValue = new ReturnValue();

        public AsyncUploadBlockHandler(string userId, string fileId, int blockId) : base(Path.GetTempPath())
        {
            _userId = userId;
            _fileId = fileId;
            _blockId = blockId;
        }

        public override Task ExecutePostProcessingAsync()
        {
            foreach (var fileData in FileData)
            {
                var connectionString = RoleEnvironment.GetConfigurationSettingValue(Constants.ConfigurationSectionKey);
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference("FileUpload");
                var retrieveOperation = TableOperation.Retrieve<FileUpload>(_userId, _fileId);
                var retrievedResult = table.Execute(retrieveOperation);

                if (retrievedResult.Result != null)
                {
                    var fileUpload = (FileUpload)retrievedResult.Result;
                    var b64BlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", _blockId)));
                    var stream = File.OpenRead(fileData.LocalFileName);
                    fileUpload.BlockBlob.PutBlock(b64BlockId, stream, "");
                    
                    fileUpload.UploadStatusMessage = "finished upload of block " + _blockId;
                    

                    if (_blockId == fileUpload.BlockCount)
                    {
                        ReturnValue.IsLastBlock = true;
                        fileUpload.IsUploadCompleted = true;

                        try
                        {
                            var blockList = Enumerable.Range(1, (int)fileUpload.BlockCount).ToList<int>().ConvertAll(rangeElement => Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", rangeElement))));
                            fileUpload.BlockBlob.PutBlockList(blockList);
                            var duration = DateTime.Now - fileUpload.StartTime;
                            float fileSizeInKb = fileUpload.FileSize / Constants.BytesPerKb;
                            string fileSizeMessage = fileSizeInKb > Constants.BytesPerKb ?
                                string.Concat((fileSizeInKb / Constants.BytesPerKb).ToString(CultureInfo.CurrentCulture), " MB") :
                                string.Concat(fileSizeInKb.ToString(CultureInfo.CurrentCulture), " KB");
                            fileUpload.UploadStatusMessage = "Finished with file upload. total duration: " + duration + " total upload size: " + fileSizeMessage;

                            //now that we are done with the upload, we can put the file in to the processer queue
                            var file = new UploadQueue
                                {
                                    FileID = fileUpload.FileId,
                                    OriginalFileName = fileUpload.FileName,
                                    FileLocation = fileUpload.BlockBlob.Uri.ToString(),
                                    ContentType = "application/zip", //TODO: I should try and actually figure out what the ContentType is.
                                    ContentSize = fileUpload.FileSize,
                                    UserKey = fileUpload.UserId,
                                    UserIP = fileUpload.UserIP,
                                    UploadedDate = fileUpload.StartTime,
                                    Processing = false
                                };

                            EVSAppController.Controllers.UploadQueueController.AddItemToQueue(file);
                        }
                        catch (Exception exc)
                        {
                            ReturnValue.ErrorInOperation = true;
                            fileUpload.UploadStatusMessage = "There was an error placing the final block list.";
                        }
                    }

                    var updateOperation = TableOperation.Replace(fileUpload);
                    table.Execute(updateOperation);

                    ReturnValue.Message = fileUpload.UploadStatusMessage;
                }
                else
                {
                    ReturnValue.ErrorInOperation = true;
                    ReturnValue.Message = "File could not be located in FileUpload table.";
                }
            }
            
            return base.ExecutePostProcessingAsync();
        }
    }
}