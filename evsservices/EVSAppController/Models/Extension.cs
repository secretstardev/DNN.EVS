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