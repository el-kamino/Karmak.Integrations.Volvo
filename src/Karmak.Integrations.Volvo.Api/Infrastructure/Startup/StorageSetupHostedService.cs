using Karmak.Integrations.Volvo.Common.BlobClient;

namespace Karmak.Integrations.Volvo.Api.Infrastructure.Startup
{
    public class StorageSetupHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public StorageSetupHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var claimCheck = _serviceProvider.GetRequiredKeyedService<IKarmakBlobClient>("ClaimCheck");
            await claimCheck.CreateContainerAsync();

            var volvoReact = _serviceProvider.GetRequiredKeyedService<IKarmakBlobClient>("VolvoReact");
            await volvoReact.CreateContainerAsync();

            var standardCodes = _serviceProvider.GetRequiredKeyedService<IKarmakBlobClient>("StandardCodes");
            await standardCodes.CreateContainerAsync();

            var symptomCodes = _serviceProvider.GetRequiredKeyedService<IKarmakBlobClient>("SymptomCodes");
            await symptomCodes.CreateContainerAsync();

            var externalClient = _serviceProvider.GetRequiredService<IExternalBlobClient>();
            await externalClient.CreateContainerAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
