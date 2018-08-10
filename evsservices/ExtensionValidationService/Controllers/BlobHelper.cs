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

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ExtensionValidationService.Controllers
{
    //TODO:remove this if not needed
    //internal static class BlobHelper
    //{
    //    public static CloudBlobContainer GetContainer(string containerName)
    //    {
    //        // Retrieve storage account from connection-string
    //        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("CloudStorageConnectionString"));

    //        // Create the blob client 
    //        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

    //        // Retrieve a reference to a container 
    //        // Container name must use lower case
    //        CloudBlobContainer container = blobClient.GetContainerReference(containerName);

    //        // Create the container if it doesn't already exist
    //        container.CreateIfNotExists();

    //        // Enable public access to blob
    //        var permissions = container.GetPermissions();
    //        if (permissions.PublicAccess == BlobContainerPublicAccessType.Off)
    //        {
    //            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
    //            container.SetPermissions(permissions);
    //        }

    //        return container;
    //    }
    //}
}