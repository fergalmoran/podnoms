using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PodNoms.Api.Models.Settings;

namespace PodNoms.Api.Services.Storage {
    internal class AzureFileUtilities : IFileUtilities {
        private readonly StorageSettings _settings;
        public AzureFileUtilities(IOptions<StorageSettings> settings, ILoggerFactory logger) {
            this._settings = settings.Value;
        }
        public async Task<long> GetRemoteFileSize(string containerName, string fileName) {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_settings.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            var exists = await container.ExistsAsync();
            if (exists) {
                CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
                exists = await blob.ExistsAsync();
                if (exists) {
                    await blob.FetchAttributesAsync();
                    return blob.Properties.Length;
                }
            }
            return -1;
        }
    }
}