using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace EVSAppController
{
    public class FileHelpers
    {
        public static string MoveBlobToTempStorage(string blobURI)
        {
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue(Constants.ConfigurationSectionKey));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(Constants.ContainerName);
            var blob = container.GetBlobReference(blobURI);
            var file = blob.DownloadByteArray();
            var filepath = Path.GetTempFileName();

            File.WriteAllBytes(filepath, file);

            return filepath;
        }

        public static void RemoveBlob(string blobURI)
        {
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue(Constants.ConfigurationSectionKey));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(Constants.ContainerName);
            var blob = container.GetBlobReference(blobURI);
            blob.Delete();
        }
    }
}