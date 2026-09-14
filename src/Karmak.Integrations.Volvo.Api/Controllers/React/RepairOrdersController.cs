using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Api.Models.React;
using Karmak.Integrations.Volvo.Fusion.React.RepairOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers.React;

[ApiController]
[Authorize(AuthenticationSchemes = SecurityExtensions.MicrosoftScheme)]
public class RepairOrdersController : ControllerBase
{
    private readonly IRepairOrderRetransmitDispatcher _dispatcher;

    public RepairOrdersController(
        IRepairOrderRetransmitDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    [Route("/repair_order/retransmission")]
    [Authorize(Roles = "React.Retransmit")]
    public async Task<ActionResult> SubmitRepairOrderRetransmissionRequest([FromBody] RepairOrderRetransmitRequest request)
    {
        var correlationId = await _dispatcher.RetransmitRepairOrdersAsync(request.PaCode, request.WindowStart, request.WindowEnd, request.RepairOrderNumbers);
        return Ok(new { CorrelationId = correlationId });
    }
}
