using AutoMapper;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Fusion.React.PartsInventory;

internal class PartsInventoryDispatcher : IRequestDispatcher
{
    public const string EntityType = "Volvo Parts Inventory";

    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDealerInfoExtractor _dealerInfoExtractor;
    private readonly IKarmakBlobClient _claimCheckClient;
    private readonly IExternalBlobClient _externalBlobClient;

    public PartsInventoryDispatcher(
        IMapper mapper,
        IPublishEndpoint publishEndpoint,
        IDealerInfoExtractor dealerInfoExtractor,
        [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
        IExternalBlobClient externalBlobClient)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _dealerInfoExtractor = dealerInfoExtractor;
        _claimCheckClient = claimCheckClient;
        _externalBlobClient = externalBlobClient;
    }

    public async Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
    {
        string blobName = request.Payload["BlobName"]?.Value<string>();

        if (string.IsNullOrWhiteSpace(blobName))
        {
            throw new UnrecoverableFusionRequestException(new ArgumentException("BlobName cannot be null or whitespace"));
        }

        var inventory = await _externalBlobClient.RetrieveAsync<FusionPartsInventoryReport>(blobName);

        var sbPayload = _mapper.Map<FusionPartsInventoryReport, PartsInventoryReport>(
            inventory,
            options =>
            {
                options.Items[Constants.TimeZone] = request.CreatedTimeZone;
            });

        sbPayload.DealerInfo = _dealerInfoExtractor.Extract();

        string claimCheckBlobName = await _claimCheckClient.UploadAsync(sbPayload);
        await _publishEndpoint.Publish(
            new PartsInventoryReportReceived
            {
                ReportClaimCheck = new PartsInventoryReportClaimCheck
                {
                    BlobName = claimCheckBlobName
                }
            });
    }
}