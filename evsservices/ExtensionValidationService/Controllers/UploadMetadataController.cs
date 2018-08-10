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
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using ExtensionValidationService.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using EVSAppController;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace ExtensionValidationService.Controllers
{
    public class UploadMetadataController: ApiController
    {
        public HttpResponseMessage Post()
        {
            var nvc = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            int blocksCount;
            int.TryParse(nvc["blocksCount"], out blocksCount);
            var fileName = nvc["fileName"];
            long fileSize;
            long.TryParse(nvc["fileSize"], out fileSize);
            var userId = nvc["userId"];
            var fileId = Guid.NewGuid().ToString();
            var newFileName = fileId + ".dat";
            var connectionString = RoleEnvironment.GetConfigurationSettingValue(Constants.ConfigurationSectionKey);
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var fileToUpload = new FileUpload(userId, fileId)
            {
                UserIP = ClientIP.GetClientIp(Request),
                BlockCount = blocksCount,
                FileName = fileName,
                NewFileName = newFileName,
                FileSize = fileSize,
                StartTime = DateTime.Now,
                IsUploadCompleted = false,
                UploadStatusMessage = "Starting Upload"
            };

            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("FileUpload");

            table.CreateIfNotExists();
            var insertOperation = TableOperation.Insert(fileToUpload);
            table.Execute(insertOperation);

            var resp = Request.CreateResponse(HttpStatusCode.OK);
            
            var metaDataOutput = new MetaDataOutput {Success = true, FileId = fileToUpload.FileId};

            resp.Content = new ObjectContent(typeof(MetaDataOutput), metaDataOutput, new JsonMediaTypeFormatter());
            resp.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST");
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "x-requested-with");
            return resp;
        }

        public HttpResponseMessage Options()
        {
            var resp = Request.CreateResponse(HttpStatusCode.OK);
            resp.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST");
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Headers", "x-requested-with");
            return resp;
        }

        public class MetaDataOutput
        {
            public bool Success { get; set; }
            public string FileId { get; set; }
        }
    }
}