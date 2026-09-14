using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Inbox.Validators.Blob
{
    public class StorageClient : IStorageClient
    {
        private readonly BlobServiceClient _cloudBlobClient;
        private readonly string _containerName;

        public StorageClient(IOptions<BlobStorageOptions> options)
        {
            _cloudBlobClient = new BlobServiceClient(options.Value.BlobConnectionString);
            _containerName = options.Value.ContainerName;
        }

        public async Task<string> UploadAsync<TEntity>(BlobUploadRequest<TEntity> entityToUpload)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entityToUpload?.BlobName);

            BlobContainerClient container = _cloudBlobClient.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            BlobClient blob = container.GetBlobClient(entityToUpload.BlobName);

            var options = new BlobUploadOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["OriginalFileName"] = entityToUpload.OriginalFileName ?? "unknown",
                    ["FileType"] = entityToUpload.FileType ?? "unknown"
                }
            };

            var data = new BinaryData(entityToUpload.ContentBuffer);
            await blob.UploadAsync(data, options);

            return blob.Uri.ToString();
        }

        public async Task<TEntity> DownloadAsync<TEntity>(string uri)
        {
            var blob = new BlobClient(new Uri(uri));

            using (var ms = new MemoryStream())
            {
                var contents = await blob.DownloadToAsync(ms);
                ms.Position = 0;

                using(var sr = new StreamReader(ms))
                using(var jtr = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    TEntity result = serializer.Deserialize<TEntity>(jtr)!;
                    return result;
                }
            }
        }
    }
}