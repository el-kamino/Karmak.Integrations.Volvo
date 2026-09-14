using System.Net.Mime;
using System.Xml;
using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers.Warranty
{
    [ApiController]
    [Route("/api/VolvoWarrantyService/v1/Warranty/push")]
    [Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
    public class PushController : ControllerBase
    {
        private readonly IVolvoPushUpdateService _service;

        public PushController(IVolvoPushUpdateService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpPost]
        [Produces("application/xml")]
        public async Task<IActionResult> Post()
        {
            var pushUpdate = await TryReadPushUpdate();
            var (success, response) = await _service.HandleUpdate(pushUpdate);
            return new ContentResult
            {
                StatusCode = success ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError,
                ContentType = MediaTypeNames.Application.Xml,
                Content = response.OuterXml
            };
        }

        private async Task<XmlDocument> TryReadPushUpdate()
        {
            var requestBody = new XmlDocument
            {
                PreserveWhitespace = true
            };

            using (var stream = new StreamReader(Request.Body))
            {
                requestBody.LoadXml(await stream.ReadToEndAsync());
            }

            return requestBody;
        }
    }
}
