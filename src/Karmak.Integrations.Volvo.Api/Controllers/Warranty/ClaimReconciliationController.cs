using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers.Warranty
{
    [ApiController]
    [Route("/api/VolvoWarrantyService/v1/Warranty/claims-reconciliations")]
    [Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
    public class ClaimReconciliationController : ControllerBase
    {
        private readonly IVolvoWarrantyReconcilliationFetchService _service;
        private readonly IInboundMessageSender _sender;
        private readonly ILogger<ClaimReconciliationController> _logger;

        public ClaimReconciliationController(IVolvoWarrantyReconcilliationFetchService service, IInboundMessageSender sender, ILogger<ClaimReconciliationController> logger)
        {
            _service = service;
            _sender = sender;
            _logger = logger;
        }

        [HttpPost("fetch")]
        public async Task<ActionResult> Fetch([FromBody] ReconciliationFetchRequest claimsReconciliationsFetchRequest)
        {
            var validationResult = new ReconciliationFetchRequestValidator().Validate(claimsReconciliationsFetchRequest);
            if (!validationResult.IsValid)
            {
                return new BadRequestResult();
            }
            var (success, _) = await _service.TriggerReconciliationFetch(claimsReconciliationsFetchRequest);

            if (!success)
            {
                return StatusCode(500);
            }

            return new AcceptedResult();
        }

        [HttpPost("process")]
        public async Task<ActionResult> Process([FromBody] UpdateSnapshotMessage request)
        {
            _logger.LogInformationWithMetadata("Processing reconciliation update.", new Dictionary<string, string> { { "ClaimId", request.ClaimId }, { "UpdateSnapshotId", request.UpdateSnapshot.Id } });

            try
            {
                await _sender.PublishAsync(request);
                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
    }
}
