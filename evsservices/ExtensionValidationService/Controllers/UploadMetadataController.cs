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