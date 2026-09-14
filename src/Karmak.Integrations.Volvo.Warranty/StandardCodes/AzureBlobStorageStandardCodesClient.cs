using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Warranty.StandardCodes
{
    public class AzureBlobStorageStandardCodesClient : IStandardCodesClient
    {
        private readonly IKarmakBlobClient _blobClient;
        private readonly IStandardCodesClient _fallbackClient;
        private readonly ILogger<AzureBlobStorageStandardCodesClient> _logger;
        private readonly IDateTimeOffsetProvider _dateTimeProvider;
        private readonly StandardCodesBlobCacheOptions _cacheOptions;
        private const string FileExtension = ".json";

        public AzureBlobStorageStandardCodesClient(
            IKarmakBlobClient blobClient,
            IStandardCodesClient fallbackClient,
            ILogger<AzureBlobStorageStandardCodesClient> logger,
            IDateTimeOffsetProvider dateTimeProvider,
            IOptions<StandardCodesBlobCacheOptions> cacheOptions)
        {
            _blobClient = blobClient;
            _fallbackClient = fallbackClient;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _cacheOptions = cacheOptions.Value;
        }

        public async Task<IEnumerable<StandardCode>> FetchCodesAsync(StandardCodesSettings standardCodesSettings)
        {
            _logger.LogInformation("Attempting to fetch Standard Codes from level one cache.");
            var blobName = BuildFileName(standardCodesSettings);

            if (await _blobClient.BlobExistsAsync(blobName))
            {
                var cached = await _blobClient.RetrieveAsync<CachedStandardCodes>(blobName);

                if (IsWithinTtl(cached.CachedAt))
                {
                    return cached.Codes;
                }

                try
                {
                    return await RefreshCacheAsync(standardCodesSettings, blobName);
                }
                catch (Exception e)
                {
                    _logger.LogInformation("OWS_StandardCodes_Fetch_Failure");
                    _logger.LogError(e, e.Message);
                    return cached.Codes;
                }
            }

            return await RefreshCacheAsync(standardCodesSettings, blobName);
        }

        private async Task<IEnumerable<StandardCode>> RefreshCacheAsync(StandardCodesSettings standardCodesSettings, string blobName)
        {
            _logger.LogInformation("Updating Standard Codes level one cache.");
            var codes = await _fallbackClient.FetchCodesAsync(standardCodesSettings);

            await _blobClient.UploadAsync(blobName, new CachedStandardCodes
            {
                Codes = codes,
                CachedAt = _dateTimeProvider.UtcNow
            });

            return codes;
        }

        private bool IsWithinTtl(DateTimeOffset cachedAt)
        {
            return (_dateTimeProvider.UtcNow - cachedAt).TotalHours < _cacheOptions.TimeToLiveHours;
        }

        private static string BuildFileName(StandardCodesSettings standardCodesSettings)
        {
            return standardCodesSettings.LanguageCode.ToLower() + FileExtension;
        }
    }
}
