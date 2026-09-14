namespace Karmak.Integrations.Volvo.Common.BlobClient;

public interface IExternalBlobClient
{
    Task<Uri> ReserveBlobForUploadAsync();
    Task<T> RetrieveAsync<T>(string blobName) where T : class;
    Task CreateContainerAsync();
}
