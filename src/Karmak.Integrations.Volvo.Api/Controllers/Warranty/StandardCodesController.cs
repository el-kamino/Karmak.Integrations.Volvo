using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Api.Controllers.Warranty
{
    [ApiController]
    [Route("/api/AppService/v1/Warranty/standard-codes")]
    [Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
    public class StandardCodesController : ControllerBase
    {
        private readonly IStandardCodesService _standardCodesService;

        private readonly WarrantyConfigurationOptions _options;

        public StandardCodesController(
            IStandardCodesService standardCodesService,
            IOptions<WarrantyConfigurationOptions> options)
        {
            _standardCodesService = standardCodesService;
            _options = options.Value;
        }

        [HttpGet]
        public async Task<IActionResult> FetchCodes()
        {
            if(_options.IsMockEnabled && IsMockRequest(HttpContext))
            {
                return new ObjectResult(
                    new StandardCode[] 
                    { 
                        new StandardCode { Code = "00", CodeType = "Test", Description = "Test Code" }
                    });
            }

            var codes = await _standardCodesService.FetchCodesAsync();
            return new ObjectResult(codes);
        }

        private bool IsMockRequest(HttpContext context)
        {
            var exists = bool.TryParse(context.Request.Headers["UseMock"].ToString(), out bool value);
            return exists && value;
        }
    }
}
