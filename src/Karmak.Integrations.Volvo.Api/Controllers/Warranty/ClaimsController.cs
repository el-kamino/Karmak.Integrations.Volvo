using Karmak.Integrations.Volvo.Api.Business;
using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Api.Controllers.Warranty
{
    [ApiController]
    [Route("/api/AppService/v1/Warranty/claims")]
    [Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimsService _claimsService;
        private readonly ILogger<ClaimsController> _logger;

        private readonly WarrantyConfigurationOptions _options;

        public ClaimsController(IClaimsService claimsService, ILogger<ClaimsController> logger, IOptions<WarrantyConfigurationOptions> options)
        {
            _claimsService = claimsService ?? throw new ArgumentNullException(nameof(claimsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Claim claim)
        {
            var createdClaim = await _claimsService.Create(claim, User.GetFusionIdentity());

            return new CreatedAtRouteResult(
                nameof(Get),
                new
                {
                    id = createdClaim.Id
                },
                createdClaim);
        }

        [HttpGet("{id}", Name = nameof(Get))]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var claim = await _claimsService.Find(id);

            if (claim == null || claim.IsDeleted)
            {
                return new NotFoundResult();
            }

            return new ObjectResult(claim);
        }

        [HttpGet("claimId/{id}", Name = nameof(GetCorrelatingClaims))]
        public async Task<IActionResult> GetCorrelatingClaims([FromRoute] string id)
        {
            var claim = await _claimsService.Find(id);

            if (claim == null || claim.IsDeleted)
            {
                _logger.LogInformationWithMetadata("Unable to find non-deleted claim with given id.",
                    new Dictionary<string, string> { { "Claim.Id", id } });
                return new NotFoundResult();
            }

            var claims = await _claimsService.FindByRepairOrderId(claim.RepairOrder.SecondaryIdentifier);

            _logger.LogInformationWithMetadata("Found correlating claims using given claim id.",
                new Dictionary<string, string> {
                    { "Claim.Id", id },
                    { "CorrelatingClaimCount", claims.Count.ToString() },
                    { "Claim.CorrelationId", claim?.CorrelationId },
                    { "Claim.RepairOrder.SecondaryIdentifier", claim.RepairOrder.SecondaryIdentifier }
                });

            return new ObjectResult(claims);
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] string id, [FromBody] ClaimStatus status)
        {
            var claim = await _claimsService.UpdateStatus(id, status, User.GetFusionIdentity());

            return new ObjectResult(claim);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromBody] Claim claim, [FromRoute] string id)
        {
            var updatedClaim = await _claimsService.Update(claim, id, User.GetFusionIdentity());

            if (updatedClaim == null)
            {
                return new ConflictResult();
            }

            return new ObjectResult(updatedClaim);
        }

        [HttpPut]
        public async Task<IActionResult> BatchUpdate([FromBody] IEnumerable<Claim> claims)
        {
            var updatedClaims = new List<Claim>();
            foreach (var x in claims)
            {
                var updatedClaim = await _claimsService.Update(x, x.Id, User.GetFusionIdentity());

                if (updatedClaim == null)
                    return new ConflictResult();

                updatedClaims.Add(updatedClaim);
            }

            return new ObjectResult(updatedClaims);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            await _claimsService.Delete(id, User.GetFusionIdentity());

            return new NoContentResult();
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit([FromRoute] string id)
        {
            var claim = await _claimsService.Find(id);

            if (claim == null)
            {
                return new NotFoundResult();
            }
            if (!(_options.IsMockEnabled && IsMockRequest(HttpContext)))
            {
                await _claimsService.Submit(claim);
            }

            return new AcceptedResult();
        }

        private bool IsMockRequest(HttpContext context)
        {
            var exists = bool.TryParse(context.Request.Headers["UseMock"].ToString(), out bool value);
            return exists && value;
        }
    }
}
