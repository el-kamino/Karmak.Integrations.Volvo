using NSubstitute;
using Karmak.Integrations.Volvo.Oasis.Services;
using Karmak.Integrations.Volvo.Oasis.Configuration;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Oasis.Tests.Services
{
    public class SymptomCodeProviderTest
    {
        [Fact]
        public async Task CanLoadCsvFile()
        {
            var path = "testpath";
            var storageClient = Substitute.For<IKarmakBlobClient>();

            storageClient.RetrieveContentsAsync(path)
                .Returns(Task.FromResult(SampleOwsSoapResponses.SymptomCodesCsv));

            var provider = new SymptomCodeProvider(storageClient, Options.Create(new SymptomCodeProviderOptions { BlobPath = path }));

            var codes = await provider.GetSymptomCodesAsync();

            Assert.True(codes.Any());
            Assert.True(codes.All(c => !string.IsNullOrWhiteSpace(c.Code) && !string.IsNullOrWhiteSpace(c.Description) && string.IsNullOrWhiteSpace(c.CodeType)));
        }

        [Fact]
        public async Task LoadCsvFileIgnoresRowsWithTooManyColumnsAndTheHeaderAndPreservesWhitespace()
        {
            var path = "testpath";
            var storageClient = Substitute.For<IKarmakBlobClient>();
            storageClient.RetrieveContentsAsync(path)
                .Returns(Task.FromResult(SampleOwsSoapResponses.SymptomCodesCsvWith3ColumnsRow));

            var provider = new SymptomCodeProvider(storageClient, Options.Create(new SymptomCodeProviderOptions { BlobPath = path }));

            var codes = await provider.GetSymptomCodesAsync();

            Assert.True(codes.Count == 1);
            Assert.Contains(codes, c => c.Code == "100000" && c.Description == "          BODY");
        }
    }
}
