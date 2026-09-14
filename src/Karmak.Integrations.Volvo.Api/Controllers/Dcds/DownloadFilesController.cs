using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Dcds.FileDownload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Karmak.Integrations.Volvo.Api.Controllers.Dcds
{
    [ApiController]
    [Authorize(AuthenticationSchemes = SecurityExtensions.MicrosoftScheme, Roles = "Dcds.Write")]
    [Route("api/dcds/[controller]")]
    public class DownloadFilesController : ControllerBase
    {
        private readonly IDownloadFilesProcessor _downloadFilesProcessor;

        public DownloadFilesController(IDownloadFilesProcessor downlaodFilesProcessor)
        {
            _downloadFilesProcessor = downlaodFilesProcessor;
        }

        [HttpPost]
        public async Task<IActionResult> DownloadFiles()
        {
            var success = await _downloadFilesProcessor.ProcessAsync();

            if (success)
                return Ok();

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}
