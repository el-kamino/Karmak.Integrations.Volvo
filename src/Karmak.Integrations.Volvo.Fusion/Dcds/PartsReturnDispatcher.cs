using AutoMapper;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsReturn;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Fusion.Dcds
{
    internal class PartsReturnDispatcher : IRequestDispatcher
    {
        public const string EntityType = "Part Purchase Order";

        private readonly IMapper _mapper;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IKarmakBlobClient _claimCheckClient;

        public PartsReturnDispatcher(
            IMapper mapper,
            ISettingsProvider settingsProvider,
            IPublishEndpoint publishEndpoint,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
        {
            _mapper = mapper;
            _settingsProvider = settingsProvider;
            _publishEndpoint = publishEndpoint;
            _claimCheckClient = claimCheckClient;
        }

        public async Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
        {
            var fusionRequestPayload = request.Payload.ToObject<PartPurchaseOrder>();

            var sbPayload = _mapper.Map<PartPurchaseOrder, PartsReturnRequest>(
                fusionRequestPayload, 
                options =>
                {
                    options.Items[Constants.FusionIdentityKey] = identity;
                });

            VolvoSettings settings = await _settingsProvider.GetSettingsAsync();
            sbPayload.PACode = settings.InterfaceOptions.PaCode;
            sbPayload.VendorId = settings.DealerServiceProviderSettings?.VendorIdentifier;
            sbPayload.DistributionCode = settings.InterfaceOptions?.SalesZoneCode;

            string blobName = await _claimCheckClient.UploadAsync(sbPayload);
            await _publishEndpoint.Publish(new PartsReturnRequestReceived { BlobName = blobName });
        }
    }
}
