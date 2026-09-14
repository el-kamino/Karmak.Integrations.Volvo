using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Oasis.Models;
using Karmak.Integrations.Volvo.Oasis.Services;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Integrations.Oasis.Web.Controllers
{
    [Route("/api/integrations-oasis/v1/Oasis/oasis")]
    [ApiController]
    [Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
    public class OasisController : ControllerBase
    {
        private readonly IStandardCodesService _standardCodesService;
        private readonly ISymptomCodeProvider _symptomCodeProvider;
        private readonly IOasisDataProvider _oasisDataProvider;

        public OasisController(
            IStandardCodesService standardCodesService,
            ISymptomCodeProvider symptomCodeProvider,
            IOasisDataProvider oasisDataProvider)
        {
            _standardCodesService = standardCodesService;
            _symptomCodeProvider = symptomCodeProvider;
            _oasisDataProvider = oasisDataProvider;
        }

        [HttpPost]
        public async Task<IActionResult> GetOasisData([FromBody] OasisRequestRest request)
        {
            var response = await _oasisDataProvider.GetDataAsync(request);

            return Ok(new
            {
                Payload = string.IsNullOrWhiteSpace(response?.Payload) ? null : JObject.Parse(response.Payload)
            });
        }

        [HttpGet("symptomcodes")]
        public async Task<IActionResult> GetSymptomCodes()
        {
            var symptomCodes = await _symptomCodeProvider.GetSymptomCodesAsync();
            return Ok(symptomCodes);
        }

        [HttpGet("complaintcodes")]
        public async Task<IActionResult> GetComplaintCodes()
        {
            var complaintCodes = await _standardCodesService.FetchCodesAsync();
            return Ok(complaintCodes);
        }
    }
}
