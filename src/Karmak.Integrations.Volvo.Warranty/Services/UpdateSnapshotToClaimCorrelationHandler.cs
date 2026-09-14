using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public class UpdateSnapshotToClaimCorrelationHandler : IUpdateSnapshotCorrelationHandler
    {
        private readonly IClaimsService _claimsService;
        private readonly ILogger<UpdateSnapshotToClaimCorrelationHandler> _logger;
        public UpdateSnapshotToClaimCorrelationHandler(IClaimsService claimsService, ILogger<UpdateSnapshotToClaimCorrelationHandler> logger)
        {
            _claimsService = claimsService ?? throw new ArgumentNullException(nameof(claimsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //if there is more than 1 claim, check if correlation id is set
        public async Task<IEnumerable<Claim>> Correlate(string repairOrderId)
        {
            var telemetryInfo = new Dictionary<string, string> {
                { "RepairOrder.Id", repairOrderId }
            };

            var claims = await _claimsService.FindByRepairOrderId(repairOrderId);

            switch (claims?.Count)
            {
                case null:
                case 0:
                    _logger.LogInformationWithMetadata("No claims were found.", telemetryInfo);

                    return null;
                case 1:
                    telemetryInfo.Add("Claim.Id", claims.First().Id);
                    telemetryInfo.Add("Correlation.Id", claims.First().CorrelationId);
                    _logger.LogInformationWithMetadata("1 claim was found.", telemetryInfo);

                    return claims;
                default:
                    string correlationId = claims.First().CorrelationId;
                    if (!string.IsNullOrEmpty(correlationId))
                    {
                        bool allCorrelationIdsMatch = claims.All(claim => correlationId.Equals(claim.CorrelationId));
                        if (allCorrelationIdsMatch)
                        {
                            telemetryInfo.Add("Correlation.Id", claims.First().CorrelationId);
                            _logger.LogInformationWithMetadata($"{claims?.Count} claims were found.", telemetryInfo);

                            return claims;
                        }
                        else
                        {
                            _logger.LogInformationWithMetadata($"{claims?.Count} claims were found, but not all Correlation IDs matched.", telemetryInfo);
                        }
                    }
                    else
                    {
                        _logger.LogInformationWithMetadata($"{claims?.Count} claims were found, but not all Correlation IDs were set.", telemetryInfo);
                    }
                    return null;
            }
        }
    }
}
