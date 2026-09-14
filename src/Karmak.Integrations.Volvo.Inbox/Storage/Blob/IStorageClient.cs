namespace Karmak.Integrations.Volvo.Inbox.Validators.Blob;

public interface IStorageClient
{
    Task<string> UploadAsync<TEntity>(BlobUploadRequest<TEntity> entityToUpload);
    Task<TEntity> DownloadAsync<TEntity>(string uri);
}