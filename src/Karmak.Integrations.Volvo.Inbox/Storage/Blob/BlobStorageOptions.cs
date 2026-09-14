namespace Karmak.Integrations.Volvo.Inbox.Validators.Blob
{
    public class BlobStorageOptions
    {
        public required string BlobConnectionString { get; set; }
        public required string ContainerName { get; set; }
    }
}
