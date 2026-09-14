using Karmak.Integrations.Volvo.Common.BlobClient;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers
{
    [Route("/UI/v1/Oasis")]
    public class OasisUIController : UIControllerBase
    {
        public OasisUIController(
            [FromKeyedServices("OasisUIBlobClient")] IKarmakBlobClient inboxBlobClient)
            : base(inboxBlobClient)
        {
        }

        [HttpGet("{filename?}")]
        public async Task<IActionResult> Get(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = "index.html";
            }

            return await GetBlobAsync(filename);
        }
    }
}
