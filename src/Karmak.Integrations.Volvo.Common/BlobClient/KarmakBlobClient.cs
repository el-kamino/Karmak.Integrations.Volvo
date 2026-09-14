using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using AzureBlobClient = Azure.Storage.Blobs.BlobClient;

namespace Karmak.Integrations.Volvo.Common.BlobClient;

public class KarmakBlobClient : IKarmakBlobClient
{
    protected readonly BlobServiceClient _client;
    protected readonly string _containerName;

    public KarmakBlobClient(IOptions<KarmakBlobClientOptions> options)
    {
        _client = new BlobServiceClient(options.Value.StorageConnectionString);
        _containerName = options.Value.ContainerName;
    }

    public async Task<string> UploadAsync<T>(T data)
    {
        return await UploadAsync(Guid.NewGuid().ToString("d"), data);
    }

    public async Task<string> UploadAsync<T>(string blobName, T data)
    {
        AzureBlobClient blobClient = GetBlobClient(blobName);

        using (var writeStream = await blobClient.OpenWriteAsync(overwrite: true))
        {
            await System.Text.Json.JsonSerializer.SerializeAsync(writeStream, data);
        }

        return blobClient.Name;
    }

    public async Task<T> RetrieveAsync<T>(string blobName)
        where T : class
    {
        AzureBlobClient blobClient = GetBlobClient(blobName);

        using (var readStream = await blobClient.OpenReadAsync())
        {
            T data = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(readStream);
            return data;
        }
    }

    public async Task<string> RetrieveContentsAsync(string blobName)
    {
        AzureBlobClient blobClient = GetBlobClient(blobName);

        using (var readStream = await blobClient.OpenReadAsync())
        using(var streamReader = new StreamReader(readStream))
        {
            string contents = await streamReader.ReadToEndAsync();
            return contents;
        }
    }

    public async Task<bool> BlobExistsAsync(string blobName) => await _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(blobName)
            .ExistsAsync();

    public async Task CreateContainerAsync()
    {
        await _client.GetBlobContainerClient(_containerName).CreateIfNotExistsAsync();
    }

    public async Task<BlobDownloadStreamingResult> DownloadBlobAsync(string blobName)
    {
        AzureBlobClient client = GetBlobClient(blobName);
        BlobDownloadStreamingResult download = await client.DownloadStreamingAsync();
        return download;
    }

    private AzureBlobClient GetBlobClient(string blobName)
    {
        return _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(blobName);
    }
}
