using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Dcds;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.FinancialStatement;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Fusion.Dcds
{
    internal class FinancialStatementDispatcher : IRequestDispatcher
    {
        public const string EntityType = "Volvo Financial Statement";

        private readonly ISettingsProvider _settingsProvider;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IKarmakBlobClient _claimCheckClient;

        public FinancialStatementDispatcher(
            ISettingsProvider settingsProvider,
            IPublishEndpoint publishEndpoint,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
        {
            _settingsProvider = settingsProvider;
            _publishEndpoint = publishEndpoint;
            _claimCheckClient = claimCheckClient;
        }

        public async Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
        {
            var fusionRequestPayload = request.Payload.ToObject<FusionVolvoFinancialStatementRequest>();

            var message = new FinancialStatementRequest
            {
                FormattedFileContents = fusionRequestPayload.FinancialFileText,
                RequestDate = Utility.AtOffsetInHours(request.CreatedDateTime, request.CreatedTimeZone),
                KarmakAccountNumber = identity.AccountCode
            };

            VolvoSettings settings = await _settingsProvider.GetSettingsAsync();
            message.PACode = settings.InterfaceOptions.PaCode;

            string blobName = await _claimCheckClient.UploadAsync(message);
            await _publishEndpoint.Publish(new FinancialStatementRequestReceived { BlobName = blobName });
        }
    }
}
