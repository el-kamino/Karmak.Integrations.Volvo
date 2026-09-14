using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers.Warranty
{
    /// <summary>
    /// Searches claims in sql. Sits alongside <see cref="SearchController"/>, which answers the same
    /// question from the azure search index and is still the endpoint in use.
    /// </summary>
    [ApiController]
    [Route("/api/AppService/v1/Warranty/claims/search")]
    [Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
    public class ClaimSearchController : ControllerBase
    {
        private readonly IClaimSearchService _claimSearchService;

        public ClaimSearchController(IClaimSearchService claimSearchService)
        {
            _claimSearchService = claimSearchService ?? throw new ArgumentNullException(nameof(claimSearchService));
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] ClaimSearchRequest request)
        {
            try
            {
                return new ObjectResult(await _claimSearchService.Search(request));
            }
            catch (InvalidClaimSearchException e)
            {
                // The request named something the claims cannot be searched by, which is the caller's
                // to correct, so it comes back as a bad request with what was wrong with it.
                return BadRequest(e.Message);
            }
        }
    }
}
