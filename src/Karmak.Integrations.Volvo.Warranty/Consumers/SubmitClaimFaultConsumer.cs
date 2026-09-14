using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using MassTransit;
using Microsoft.Extensions.Logging;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Warranty.Consumers
{
    public class SubmitClaimFaultConsumer : IConsumer<Fault<SubmitClaimPayload>>
    {
        private readonly ILogger<SubmitClaimFaultConsumer> _logger;
        private readonly IClaimsService _claimsService;

        public SubmitClaimFaultConsumer(ILogger<SubmitClaimFaultConsumer> logger, IClaimsService claimsService)
        {
            _logger = logger;
            _claimsService = claimsService;
        }

        public async Task Consume(ConsumeContext<Fault<SubmitClaimPayload>> context)
        {
            var claims = context.Message.Message.Claims;

            var claimMetadata = new Dictionary<string, string> {
                { "Correlation.Id", claims.First().CorrelationId },
                { "Original.RepairOrder.Number", claims.First().RepairOrder.Identifier },
                { "RepairOrder.Number", claims.First().RepairOrder.SecondaryIdentifier }
            };
            for (int i = 0; i < claims.Count(); i++)
            {
                claimMetadata.Add($"Claim_{i + 1}.Id", claims.ElementAt(i).Id);
            }

            _logger.LogInformationWithMetadata($"Failed to submit claim, updating status to {nameof(ClaimStatus.Failed)}", claimMetadata);
            foreach (var claim in claims)
            {
                await _claimsService.UpdateStatus(claim.Id, ClaimStatus.Failed, FusionIdentity.Volvo);
            }
            _logger.LogInformationWithMetadata($"Updated Claim status to {nameof(ClaimStatus.Failed)}", claimMetadata);
        }
    }
}
