using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Api.Models.React;
using Karmak.Integrations.Volvo.Fusion.React.CustomerUpdate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers.React;

[ApiController]
[Authorize(AuthenticationSchemes = SecurityExtensions.MicrosoftScheme)]
public class CustomerUpdateController : ControllerBase
{
    private readonly ICustomerUpdateRetransmitDispatcher _dispatcher;

    public CustomerUpdateController(ICustomerUpdateRetransmitDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    [Authorize(Roles = "React.Retransmit")]
    [Route("/customer_update/retransmission")]
    public async Task<ActionResult> SubmitCustomerUpdateRetransmissionRequest([FromBody] CustomerUpdateRetransmitRequest request)
    {
        var response = await _dispatcher.RetransmitCustomerUpdateAsync(request.PaCode, request.WindowStart, request.WindowEnd, request.VolvoPassIds, request.VINs);
        return Ok(new { CorrelationId = response });
    }
}
