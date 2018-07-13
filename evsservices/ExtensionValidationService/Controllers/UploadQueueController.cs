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
    public class UploadQueueController: ApiController
    {
        public HttpResponseMessage Post()
        {
            var nvc = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            
            var fileName = nvc["fileName"];
            long fileSize;
            long.TryParse(nvc["fileSize"], out fileSize);
            var userIP = nvc["userIP"];
            var fileId = Guid.NewGuid().ToString();
            var fileType = nvc["fileType"];
            var fileURI = nvc["fileURI"];
            var userKey = nvc["userKey"];

            var queueItem = new EVSAppController.Models.UploadQueue();
            
            //fix to trim IPv4 addresses
            if(userIP.Contains(":"))
            {
                userIP = userIP.Split(':')[0];
            }

            queueItem.ContentSize = fileSize;
            queueItem.ContentType = fileType;
            queueItem.FileID = fileId;
            queueItem.FileLocation = fileURI;
            queueItem.OriginalFileName = fileName;
            queueItem.UploadedDate = DateTime.Now;
            queueItem.UserIP = userIP;
            queueItem.UserKey = userKey;
            queueItem.Processing = false;

            EVSAppController.Controllers.UploadQueueController.AddItemToQueue(queueItem);


            var resp = Request.CreateResponse(HttpStatusCode.OK);
            
            var metaDataOutput = new MetaDataOutput {Success = true, FileId = fileId};

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