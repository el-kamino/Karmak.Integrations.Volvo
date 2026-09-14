using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Oasis.Configuration;
using Karmak.Integrations.Volvo.Oasis.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Oasis.Services
{
    public class SymptomCodeProvider : ISymptomCodeProvider
    {
        private const char CommaDelimiter = ',';

        private readonly IKarmakBlobClient _blobStorageClient;
        private readonly SymptomCodeProviderOptions _options;

        public SymptomCodeProvider(
            [FromKeyedServices("SymptomCodes")] IKarmakBlobClient blobStorageClient,
            IOptions<SymptomCodeProviderOptions> options)
        {
            _blobStorageClient = blobStorageClient;
            _options = options.Value;
        }

        public async Task<List<CodeLookupRest>> GetSymptomCodesAsync()
        {
            var contents = await _blobStorageClient.RetrieveContentsAsync(_options.BlobPath);
            // NOTE: Delimiter is based on where the CSV file was created, Windows or Mac/Linux, to parse this out properly
            var delimiter = contents.Contains("\r\n") ? "\r\n" : "\n";
            var lines = contents.Split(delimiter);

            var codes = new List<CodeLookupRest>();

            foreach (var line in lines)
            {
                if (IsHeaderLineOrEmpty(line))
                    continue;

                var parts = line.Split(CommaDelimiter);
                if (parts.Length != 2)
                    continue;

                codes.Add(new CodeLookupRest { Code = parts[0], Description = parts[1] });
            }

            return codes;
        }

        private static bool IsHeaderLineOrEmpty(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("code,"))
                return true;

            return false;
        }
    }
}
