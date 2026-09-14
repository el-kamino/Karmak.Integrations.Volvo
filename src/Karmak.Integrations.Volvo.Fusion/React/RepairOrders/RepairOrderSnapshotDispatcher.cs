using AutoMapper;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Fusion.React.RepairOrders.MessageOrdering;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.React.Persistence.React;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Fusion.React.RepairOrders;

internal class RepairOrderSnapshotDispatcher : IRequestDispatcher
{
    public const string EntityType = "Repair Order";

    private readonly IMapper _mapper;
    private readonly IDealerInfoExtractor _dealerInfoExtractor;
    private readonly IAtomicCounter _atomicCounter;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IReactRepositoryWrapper _reactStorage;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IKarmakBlobClient _claimCheckClient;

    public RepairOrderSnapshotDispatcher(
        IMapper mapper, 
        IDealerInfoExtractor dealerInfoExtractor, 
        IAtomicCounter atomicCounter,
        IPublishEndpoint sendEndpointProvider,
        IReactRepositoryWrapper reactStorage,
        ISettingsProvider settings,
        [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
    {
        _mapper = mapper;
        _dealerInfoExtractor = dealerInfoExtractor;
        _atomicCounter = atomicCounter;
        _publishEndpoint = sendEndpointProvider;
        _reactStorage = reactStorage;
        _settingsProvider = settings;
        _claimCheckClient = claimCheckClient;
    }

    public async Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
    {
        var requestPayload = request.Payload.ToObject<FusionRepairOrderSnapshot>();
        var sbPayload = _mapper.Map<FusionRepairOrderSnapshot, RepairOrderSnapshot>(
            requestPayload, 
            options =>
            {
                options.Items[Constants.TimeZone] = request.CreatedTimeZone;
                options.Items[Constants.FusionIdentityKey] = identity;
            });

        DealerInfo dealerInfo = _dealerInfoExtractor.Extract();
        sbPayload.DealerInfo = dealerInfo;
        sbPayload.SnapshotSequenceNumber = await GetIncrementedSequenceNumber(dealerInfo.InstanceId, dealerInfo.BranchId, requestPayload.RepairOrderNumber);
        sbPayload.SnapshotSequenceNumberDateTime = request.CreatedDateTime;
        sbPayload.ForceTransmission = request.ForceProcessing;

        if (request.Action == "3")
        {
            sbPayload.RepairOrderStatus = "voided";
        }

        var settings = await _settingsProvider.GetSettingsAsync();
        await _reactStorage.CreateRepairOrderAsync(identity.AccountCode, settings.InterfaceOptions.PaCode, sbPayload);

        string blobName = await _claimCheckClient.UploadAsync(sbPayload);
        await _publishEndpoint.Publish(
            new RepairOrderReceived 
            {
                RepairOrderId = sbPayload.RepairOrderID.ToString(),
                BlobName = blobName.ToString()
            });
    }

    private async Task<int> GetIncrementedSequenceNumber(Guid instanceId, Guid branchId, string repairOrderNumber)
    {
        var partitionKey = instanceId.ToString();
        var counterName = $"BranchId:{branchId.ToString()}_RepairOrderNumber:{repairOrderNumber}";

        return await _atomicCounter.IncrementAsync(partitionKey, counterName);
    }
}
