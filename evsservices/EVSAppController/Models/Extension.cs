using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace EVSAppController.Models
{
    public class Extension
    {
        public Extension()
        {
            
        }

        public Extension(UploadQueue uploadQueueItem)
        {
            ContentSize = uploadQueueItem.ContentSize;
            ContentType = uploadQueueItem.ContentType;
            FileID = uploadQueueItem.FileID;
            FileLocation = uploadQueueItem.FileLocation;
            OriginalFileName = uploadQueueItem.OriginalFileName;
            ProcessedDate = DateTime.Now;
            UploadedDate = uploadQueueItem.UploadedDate;
            UserIP = uploadQueueItem.UserIP;
            UserKey = uploadQueueItem.UserKey;
        }

        private List<ExtensionDetail> _extensionDetails;
        private List<ExtensionMessage> _extensionMessages; 

        public int ExtensionID { get; set; }
        public string FileID { get; set; }
        public string OriginalFileName { get; set; }
        public string FileLocation { get; set; }
        public string ContentType { get; set; }
        public long ContentSize { get; set; }
        public string UserKey { get; set; }
        public string UserIP { get; set; }
        public DateTime UploadedDate { get; set; }
        public DateTime ProcessedDate { get; set; }

        [PetaPoco.Ignore]
        public List<ExtensionDetail> ExtensionDetails
        {
            get {return _extensionDetails ?? (_extensionDetails = Controllers.ExtensionDetailController.GetExtensionDetailList(ExtensionID));}
        }

        [PetaPoco.Ignore]
        public List<ExtensionMessage> ExtensionMessages
        {
            get { return _extensionMessages ?? (_extensionMessages = Controllers.ExtensionMessageController.GetExtensionMessageList(ExtensionID)); }
        }

        [PetaPoco.Ignore]
        public string SQLAzureScriptsURI
        {
            get {
                var cloudStorageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue(Constants.ConfigurationSectionKey));
                var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                var cloudBlobContainer = new CloudBlobContainer(Constants.ContainerNameScripts, cloudBlobClient);
                var fileName = FileID + ".zip";
                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                bool fileExists;

                try
                {
                    cloudBlockBlob.FetchAttributes();
                    fileExists = true;
                }
                catch (StorageClientException e)
                {
                    if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                    {
                        fileExists = false;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (fileExists)
                {
                    var sharedAccessPolicy = new SharedAccessPolicy
                    {
                        Permissions = SharedAccessPermissions.Read,
                        SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-10),
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(30)
                    };
                    return cloudBlockBlob.Uri + cloudBlockBlob.GetSharedAccessSignature(sharedAccessPolicy);
                }

                return "";
            }
        }
    }
}