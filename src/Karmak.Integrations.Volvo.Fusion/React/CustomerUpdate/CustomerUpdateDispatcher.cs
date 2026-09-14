using AutoMapper;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.CustomerUpdate;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Messages;
using Karmak.Integrations.Volvo.React.Persistence.React;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Fusion.React.CustomerUpdate;

internal class CustomerUpdateDispatcher : IRequestDispatcher
{
    public const string EntityType = "Volvo Customer Update";

    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDealerInfoExtractor _dealerInfoExtractor;
    private readonly IReactRepositoryWrapper _reactStorage;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IKarmakBlobClient _claimCheckClient;

    public CustomerUpdateDispatcher(
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
        _claimCheckClient = claimCheckClient;
        _settingsProvider = settings;
    }

    public async Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
    {
        var requestPayload = request.Payload.ToObject<FusionCustomerUpdate>();
        var sbPayload = _mapper.Map<FusionCustomerUpdate, Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate>(
            requestPayload,
            options =>
            {
                options.Items[Constants.TimeZone] = request.CreatedTimeZone;
                options.Items[Constants.FusionIdentityKey] = identity;
            });

        sbPayload.DealerInfo = _dealerInfoExtractor.Extract();
        sbPayload.ForceTransmission = request.ForceProcessing;

        switch (request.Action)
        {
            case "3":
                sbPayload.Source = "deleted";
                break;
            case "99":
                sbPayload.Source = "initial load";
                break;
        }

        var settings = await _settingsProvider.GetSettingsAsync();
        await _reactStorage.CreateCustomerUpdateAsync(identity.AccountCode, settings.InterfaceOptions.PaCode, sbPayload);

        string blobName = await _claimCheckClient.UploadAsync(sbPayload);
        await _publishEndpoint.Publish(new CustomerUpdateReceived { BlobName = blobName });
    }
}
