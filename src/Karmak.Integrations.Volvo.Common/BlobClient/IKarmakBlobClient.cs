using Azure.Storage.Blobs.Models;

namespace Karmak.Integrations.Volvo.Common.BlobClient;

public interface IKarmakBlobClient
{
    Task<string> UploadAsync<T>(T data);
    Task<string> UploadAsync<T>(string blobName, T data);
    Task<T> RetrieveAsync<T>(string blobName) where T : class;
    Task<string> RetrieveContentsAsync(string blobName);
    Task<bool> BlobExistsAsync(string blobName);
    Task CreateContainerAsync();
    Task<BlobDownloadStreamingResult> DownloadBlobAsync(string blobName);
}
