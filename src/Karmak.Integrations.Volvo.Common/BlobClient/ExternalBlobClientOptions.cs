using Azure.Core;

namespace Karmak.Integrations.Volvo.Common.BlobClient;

public class ExternalBlobClientOptions
{
    public string BlobServiceUri { get; set; }
    public string ContainerName { get; set; }
    public TokenCredential Credential { get; set; }
}
