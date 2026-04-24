using Azure.Storage.Blobs;
using MicrobloggingApp.API.Helpers;
using MicrobloggingApp.API.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MicrobloggingApp.API.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly AzureBlobSettings _settings;

        public BlobStorageService(IOptions<AzureBlobSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
        {
            var blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);

            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(fileStream, overwrite: true);
            return blobClient.Uri.ToString();
        }
    }
}
