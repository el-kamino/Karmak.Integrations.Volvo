using AutoMapper;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Messages;
using Karmak.Integrations.Volvo.React.Persistence.React;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Fusion.React.PartsSalesOrders;

internal class PartsSalesOrderDispatcher : IRequestDispatcher
{
    public const string EntityType = "Sales Order";

    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDealerInfoExtractor _dealerInfoExtractor;
    private readonly IReactRepositoryWrapper _reactStorage;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IKarmakBlobClient _claimCheckClient;

    public PartsSalesOrderDispatcher(
        IMapper mapper,
        IPublishEndpoint publishEndpoint,
        IDealerInfoExtractor dealerInfoExtractor,
        IReactRepositoryWrapper reactStorage,
        ISettingsProvider settingsProvider,
        [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _dealerInfoExtractor = dealerInfoExtractor;
        _reactStorage = reactStorage;
        _claimCheckClient = claimCheckClient;
        _settingsProvider = settingsProvider;
    }

    public async Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
    {
        var requestPayload = request.Payload.ToObject<FusionPartsSalesOrder>();
        var sbPayload = _mapper.Map<FusionPartsSalesOrder, PartsSalesOrder>(
            requestPayload,
            options =>
            {
                options.Items[Constants.TimeZone] = request.CreatedTimeZone;
                options.Items[Constants.FusionIdentityKey] = identity;
            });

        sbPayload.DealerInfo = _dealerInfoExtractor.Extract();
        sbPayload.ForceTransmission = request.ForceProcessing;
        if (request.Action == "3")
        {
            sbPayload.SalesOrderStatus = "deleted";
        }

        var settings = await _settingsProvider.GetSettingsAsync();
        await _reactStorage.CreatePartsSalesOrderAsync(identity.AccountCode, settings.InterfaceOptions.PaCode, sbPayload);

        string blobName = await _claimCheckClient.UploadAsync(sbPayload);
        await _publishEndpoint.Publish(new PartsSalesOrderReceived { BlobName = blobName });
    }
}
