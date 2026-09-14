using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using AzureBlobClient = Azure.Storage.Blobs.BlobClient;

namespace Karmak.Integrations.Volvo.Common.BlobClient
{
    public class ExternalBlobClient : IExternalBlobClient, IDisposable
    {
        //Keeping the external blob client codebase separate as it needs to stay in sync with Fusion
        //(this client uses Newtonsoft.Json to match Fusion for example) and support different
        //use cases than the internal client (e.g. SAS urls).  It makes more sense to keep this code
        //separate to prevent accidental breakage than to try and thread everything through a single
        //client codebase.

        private const string BlobResource = "b";

        private readonly KeyCache _keyCache;
        private readonly ILogger _logger;

        protected readonly BlobServiceClient _client;
        protected readonly string _containerName;

        private bool disposedValue;

        public ExternalBlobClient(
            ILogger<ExternalBlobClient> logger,
            IOptions<ExternalBlobClientOptions> options)
        {
            _client = new BlobServiceClient(new Uri(options.Value.BlobServiceUri), options.Value.Credential);
            _containerName = options.Value.ContainerName;
            _logger = logger;
            _keyCache = new KeyCache(_client, _logger);
        }

        public async Task<T> RetrieveAsync<T>(string blobName)
           where T : class
        {
            AzureBlobClient blobClient = GetBlobClient(blobName);

            using (var blobStream = await blobClient.OpenReadAsync())
            using (var streamReader = new StreamReader(blobStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                T data = serializer.Deserialize<T>(jsonReader);
                return data;
            }
        }

        public async Task<Uri> ReserveBlobForUploadAsync()
        {
            var blob = _client
                .GetBlobContainerClient(_containerName)
                .GetBlobClient(Guid.NewGuid().ToString("d"));

            var now = DateTimeOffset.UtcNow;
            var builder = new BlobSasBuilder
            {
                BlobContainerName = blob.BlobContainerName,
                BlobName = blob.Name,
                Resource = BlobResource,

                //subtract 15 minutes to account for clock skew
                StartsOn = now.AddMinutes(-15),
                ExpiresOn = now.AddHours(1)
            };

            builder.SetPermissions(BlobAccountSasPermissions.Create | BlobAccountSasPermissions.Write);

            UserDelegationKey udk = await _keyCache.GetUserDelegationKeyAsync();

            if(udk.SignedExpiresOn < builder.ExpiresOn)
            {
                throw new InvalidOperationException("signing key has shorter lifetime than sas url");
            }

            var uriBuilder = new BlobUriBuilder(blob.Uri)
            {
                Sas = builder.ToSasQueryParameters(udk, _client.AccountName)
            };

            return uriBuilder.ToUri();
        }

        public async Task CreateContainerAsync()
        {
            await _client.GetBlobContainerClient(_containerName).CreateIfNotExistsAsync();
        }

        private AzureBlobClient GetBlobClient(string blobName)
        {
            return _client
                .GetBlobContainerClient(_containerName)
                .GetBlobClient(blobName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _keyCache?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private class KeyCache : IDisposable
        {
            private const int KeyLifetimeHours = 2;

            private readonly BlobServiceClient _client;
            private readonly PeriodicTimer _refreshTimer;
            private readonly ILogger _logger;

            private Task<Response<UserDelegationKey>> _userDelegationKey;
            private bool disposedValue;

            public KeyCache(BlobServiceClient client, ILogger logger)
            {
                _client = client;
                _logger = logger;
                _refreshTimer = new PeriodicTimer(TimeSpan.FromHours(KeyLifetimeHours) / 4);

                //This will run synchronously until _userDelegationKey has been set
                _ = RefreshLoop();
            }

            public async Task<UserDelegationKey> GetUserDelegationKeyAsync()
            {
                return await _userDelegationKey;
            }

            private async Task RefreshLoop()
            {
                try
                {
                    do
                    {
                        try
                        {
                            var now = DateTimeOffset.UtcNow;
                            Task<Response<UserDelegationKey>> newKey = _client.GetUserDelegationKeyAsync(now.AddMinutes(-15), now.AddHours(KeyLifetimeHours), CancellationToken.None);

                            if (_userDelegationKey == null)
                            {
                                _userDelegationKey = newKey;
                            }
                            else
                            {
                                await newKey;
                                _ = Interlocked.Exchange(ref _userDelegationKey, newKey);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Exception refreshing user delegated access key");
                        }
                    }
                    while (await _refreshTimer.WaitForNextTickAsync());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in refresh timer. Exiting refresh loop.");
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _refreshTimer?.Dispose();
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
