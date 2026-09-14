using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Volvo.Api.Business;
using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Fusion;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace Karmak.Integrations.Volvo.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
public class FusionController : ControllerBase
{
    private readonly IExtendedLoggingService _extendedLoggingService;
    private readonly ILogger _logger;
    private readonly IServiceProvider _services;
    private readonly IExternalBlobClient _externalBlobClient;

    public FusionController(
        IExtendedLoggingService extendedLoggingService,
        IServiceProvider services,
        IExternalBlobClient externalBlobClient,
        ILogger<FusionController> logger)
    {
        _extendedLoggingService = extendedLoggingService;
        _logger = logger;
        _services = services;
        _externalBlobClient = externalBlobClient;
    }

    [HttpPost]
    [Route("/api/Core/v1/FusionAdapter/process")]
    public async Task<ActionResult> Process([FromBody] FusionRequest<JObject> request)
    {
        Activity.Current?.AddTag("volvo.react.request.entity_type", request.EntityType);
        Activity.Current?.AddTag("volvo.react.request.entity_id", request.EntityId);
        Activity.Current?.AddTag("volvo.react.request.action", request.Action);
        Activity.Current?.AddTag("volvo.react.request.created_datetime", request.CreatedDateTime.ToString());
        Activity.Current?.AddTag("volvo.react.request.created_timezone", request.CreatedTimeZone.ToString());

        try
        {
            await SaveToExtendedLoggingAsync(request);
            IRequestDispatcher dispatcher = _services.GetKeyedService<IRequestDispatcher>(request.EntityType);

            if(dispatcher == null)
            {
                string msg = $"No dispatcher found for entity type: {request.EntityType}";
                _logger.LogError(msg);
                Activity.Current?.AddException(new Exception(msg));
                return new BadRequestResult();
            }

            FusionIdentity fusionIdentity = User.GetFusionIdentity();
            await dispatcher.DispatchAsync(request, fusionIdentity);
        }
        catch (UnrecoverableFusionRequestException e)
        {
            _logger.LogError(e, $"Fusion request exception for entity type {request.EntityType}");
            Activity.Current?.AddException(e);
            return new BadRequestResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fusion exception");
            Activity.Current?.AddException(e);
            return new StatusCodeResult(500);
        }

        return new AcceptedResult();
    }

    [HttpPost]
    [Route("/api/Core/v1/FusionAdapter/claim-check/token")]
    public async Task<ActionResult> GetSasToken()
    {
        try
        {
            Uri sasToken = await _externalBlobClient.ReserveBlobForUploadAsync();
            return new OkObjectResult(sasToken.AbsoluteUri);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception generating claim check token");
            Activity.Current?.AddException(e);
            return new StatusCodeResult(500);
        }
    }

    private async Task SaveToExtendedLoggingAsync(FusionRequest<JObject> request)
    {
        var requestJson = JsonConvert.SerializeObject(request);
        var extendedLoggingRequest = new ExtendedLoggingRequest
        {
            Module = Modules.CORE,
            Application = "VolvoReact",
            Content = Encoding.Default.GetBytes(requestJson)
        };

        ExtendedLogReference extendedLogReference = await _extendedLoggingService.Execute(extendedLoggingRequest);

        Activity.Current?.AddTag("volvo.react.extended_logging.name", extendedLogReference.Name);
        Activity.Current?.AddTag("volvo.react.extended_logging.url", extendedLogReference.Uri);
    }
}
