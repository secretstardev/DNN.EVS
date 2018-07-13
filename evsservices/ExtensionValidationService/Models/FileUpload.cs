//----------------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUploadModel.cs" company="Microsoft Corporation">
//  Copyright 2011 Microsoft Corporation
// </copyright>
// Licensed under the MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (the "License"); 
// You may not use this file except in compliance with the License. 
//---------------------------------------------------------------------------------------------------------------------------

using System;
using System.Configuration;
using EVSAppController;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace ExtensionValidationService.Models
{
    /// <summary>
    /// Model denoting file object to be uploaded to blob.
    /// </summary>
    public class FileUpload : TableEntity
    {
        private CloudBlockBlob _blockBlob = null;

        /// <summary>
        /// This is a GUID that identifies the file being uploaded
        /// </summary>
        /// <value>The GUID id of the file</value>
        public string FileId { get; set; }

        /// <summary>
        /// This is a GUID that identifies the user uploading the file
        /// </summary>
        /// <value>The GUID id of the user</value>
        public string UserId { get; set; }

        /// <summary>
        /// This is the IP of the user who is uploading the file
        /// </summary>
        /// <value>the IP as a string.</value>
        public string UserIP { get; set; }

        /// <summary>
        /// Gets or sets the block count.
        /// </summary>
        /// <value>The block count.</value>
        public long BlockCount { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>The size of the file.</value>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// This is the new file name. this will prevent problems when uploading a file with the same name as a file that is already in the blob storage account.
        /// </summary>
        /// <value>The new file name of the file.</value>
        public string NewFileName { get; set; }

        /// <summary>
        /// Gets or sets the CloudBlockBlob object associated with this blob.
        /// </summary>
        /// <value>The CloudBlockBlob object associated with this blob.</value>
        public CloudBlockBlob BlockBlob
        {
            get
            {
                if(_blockBlob == null)
                {
                    var connectionString = RoleEnvironment.GetConfigurationSettingValue(Constants.ConfigurationSectionKey);
                    var storageAccount = CloudStorageAccount.Parse(connectionString);
                    var container = storageAccount.CreateCloudBlobClient().GetContainerReference(Constants.ContainerName);
                    container.CreateIfNotExists();
                    _blockBlob = container.GetBlockBlobReference(NewFileName);
                }

                return _blockBlob;
            }
            set { _blockBlob = value; }
        }

        /// <summary>
        /// Gets or sets the operation start time.
        /// </summary>
        /// <value>The start time.</value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the upload status message.
        /// </summary>
        /// <value>The upload status message.</value>
        public string UploadStatusMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether upload of this instance is complete.
        /// </summary>
        /// <value>
        /// True if upload of this instance is complete; otherwise, false.
        /// </value>
        public bool IsUploadCompleted { get; set; }

        public FileUpload(string userId, string fileId)
        {
            UserId = userId;
            FileId = fileId;

            PartitionKey = userId;
            RowKey = fileId;
        }

        public FileUpload()
        {
            
        }
}
}