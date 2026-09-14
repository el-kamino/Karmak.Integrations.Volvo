using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Api.Models.React;
using Karmak.Integrations.Volvo.Fusion.React.UsedVehicleSales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers.React;

[ApiController]
[Authorize(AuthenticationSchemes = SecurityExtensions.MicrosoftScheme)]
public class UsedVehicleSalesController : ControllerBase
{
    private readonly IUsedVehicleSalesRetransmitDispatcher _dispatcher;

    public UsedVehicleSalesController(
        IUsedVehicleSalesRetransmitDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    [Authorize(Roles = "React.Retransmit")]
    [Route("/used_vehicle_sales/retransmission")]
    public async Task<ActionResult> SubmitUsedVehicleSalesRetransmissionRequest([FromBody] VehicleSalesRetransmitRequest request)
    {
        var response = await _dispatcher.RetransmitUsedVehicleSalesAsync(request.PaCode, request.WindowStart, request.WindowEnd, request.InvoiceNumbers);
        return Ok(new { CorrelationId = response });
    }
}
