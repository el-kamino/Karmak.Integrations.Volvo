using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers
{
    [Route("healthcheck")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
