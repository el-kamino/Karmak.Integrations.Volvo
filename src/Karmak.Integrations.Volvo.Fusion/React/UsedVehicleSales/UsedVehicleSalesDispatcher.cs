using AutoMapper;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.VehicleSalesOrder;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Messages;
using Karmak.Integrations.Volvo.React.Persistence.React;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Fusion.React.UsedVehicleSales;

internal class UsedVehicleSalesDispatcher : IRequestDispatcher
{
    public const string EntityType = "Vehicle Sale";

    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDealerInfoExtractor _dealerInfoExtractor;
    private readonly IReactRepositoryWrapper _reactStorage;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IKarmakBlobClient _claimCheckClient;

    public UsedVehicleSalesDispatcher(
        IMapper mapper,
        IPublishEndpoint publishEndpoint,
        IDealerInfoExtractor dealerInfoExtractor,
        IReactRepositoryWrapper reactStorage,
        ISettingsProvider settings,
        [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _dealerInfoExtractor = dealerInfoExtractor;
        _reactStorage = reactStorage;
        _settingsProvider = settings;
        _claimCheckClient = claimCheckClient;
    }

    public async Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
    {
        var requestPayload = request.Payload.ToObject<FusionVehicleSalesOrder>();
        var sbPayload = _mapper.Map<FusionVehicleSalesOrder, VehicleSalesOrder>(
            requestPayload,
            options =>
            {
                options.Items[Constants.TimeZone] = request.CreatedTimeZone;
                options.Items[Constants.FusionIdentityKey] = identity;
            });

        sbPayload.DealerInfo = _dealerInfoExtractor.Extract();
        sbPayload.ForceTransmission = request.ForceProcessing;

        var settings = await _settingsProvider.GetSettingsAsync();
        await _reactStorage.CreateVehicleSalesOrderAsync(identity.AccountCode, settings.InterfaceOptions.PaCode, sbPayload);

        string blobName = await _claimCheckClient.UploadAsync(sbPayload);
        await _publishEndpoint.Publish(new VehicleSalesOrderReceived { BlobName = blobName.ToString() });
    }
}
