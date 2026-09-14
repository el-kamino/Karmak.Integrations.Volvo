using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Api.Models.React;
using Karmak.Integrations.Volvo.Fusion.React.PartsSalesOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers.React;

[ApiController]
[Authorize(AuthenticationSchemes = SecurityExtensions.MicrosoftScheme)]
public class PartsSalesController : ControllerBase
{
    private readonly IPartsSalesOrderRetransmitDispatcher _dispatcher;

    public PartsSalesController(
        IPartsSalesOrderRetransmitDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    [Authorize(Roles = "React.Retransmit")]
    [Route("/parts_sales_order/retransmission")]
    public async Task<ActionResult> ProcessPartsSalesOrderRetransmission([FromBody] PartsSalesRetransmitRequest request)
    {
        var response = await _dispatcher.RetransmitPartsSalesOrderAsync(request.PaCode, request.WindowStart, request.WindowEnd, request.InvoiceNumbers);
        return Ok(new { CorrelationId = response});
    }
}
