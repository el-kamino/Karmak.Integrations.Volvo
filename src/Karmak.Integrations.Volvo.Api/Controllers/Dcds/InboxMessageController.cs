using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Inbox;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Karmak.Integrations.Volvo.Api.Controllers.Dcds;

[ApiController]
[Route("api/integrations-inbox/v1/inbox/inboxmessage")]
[Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
public class InboxMessageController : ControllerBase
{
    private readonly IInboxService _inboxService;

    public InboxMessageController(IInboxService inboxService)
    {
        _inboxService = inboxService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessagesAsync([FromQuery] PagingArguments args)
    {
        var result = await _inboxService.GetAllInboxMessagesAsync(args);
        return Ok(result);
    }

    [HttpGet("contents/{id}")]
    public async Task<IActionResult> GetMessageContentsAsync(string id)
    {
        var result = await _inboxService.GetMessageContentsAsync(id);
        return Ok(result);
    }

    [HttpPost("contents")]
    public async Task<IActionResult> GetBulkMessageContentsAsync(BulkRetrieveMessageContentsArgs args)
    {
        var results = await _inboxService.GetBulkMessageContentsAsync(args);
        return Ok(results);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMessageAsync([FromBody] InboxMessageUpdateArguments args)
    {
        try
        {
            var result = await _inboxService.UpdateInboxMessageAsync(args);
            return Ok(result);
        }
        catch (Exception e)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"Unexpected Error {e.Message}");
        }
    }

    [HttpPut("bulk")]
    public async Task<IActionResult> UpdateMessagesAsync([FromBody] BulkMessageUpdateArguments args)
    {
        try
        {
            var results = await _inboxService.UpdateInboxMessagesAsync(args);
            return Ok(results);
        }
        catch (Exception e)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"Unexpected Error {e.Message}");
        }
    }
}