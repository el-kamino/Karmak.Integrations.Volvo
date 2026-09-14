using System.Net;
using Azure;
using Azure.Storage.Blobs.Models;
using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
    public class UIControllerBase : ControllerBase
    {
        private readonly IKarmakBlobClient _blobClient;

        public UIControllerBase(IKarmakBlobClient blobClient)
        {
            _blobClient = blobClient;
        }

        protected async Task<IActionResult> GetBlobAsync(string filename)
        {
            string fullPath = $"v1/{filename}";

            try
            {
                var download = await _blobClient.DownloadBlobAsync(fullPath);

                Response.Headers.ContentEncoding = download.Details.ContentEncoding;
                return File(download.Content, download.Details.ContentType);
            }
            catch(RequestFailedException rfe)
                when (rfe.Status == (int)HttpStatusCode.NotFound && rfe.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                return NotFound();
            }
        }
    }
}
