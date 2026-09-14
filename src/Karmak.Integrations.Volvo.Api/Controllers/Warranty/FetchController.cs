using FluentValidation.Results;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.ElkContextRetrieval;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Validators;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Api.Controllers.Warranty
{
    [ApiController]
    [Route("/oem/volvo-integration/v1/warranty/push")]
    public class FetchController : ControllerBase
    {
        private readonly IWarrantyElkContextRetriever _authenticationClient;
        private readonly IVolvoWarrantyReconcilliationFetchService _fetchService;
        private readonly ILogger<FetchController> _logger;

        public FetchController(
            IWarrantyElkContextRetriever authenticationClient,
            IVolvoWarrantyReconcilliationFetchService fetchService,
            ILogger<FetchController> logger)
        {
            _fetchService = fetchService ?? throw new ArgumentNullException(nameof(fetchService));
            _authenticationClient = authenticationClient;
            _logger = logger;
        }

        [HttpPost]
        [Produces("application/xml")]
        public async Task<IActionResult> Post()
        {
            var fetchRequest = await TryReadFetchRequest();
            var isFetchValid = IsFetchRequestValid(fetchRequest);

            if (!isFetchValid)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

            ElkContext context = null;
            try
            {
                context = await _authenticationClient.GetElkContextAsync(fetchRequest.PACode);

                if (context == null)
                {
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }

            var (success, _) = await ImplicitElkContext.WithCurrentAsync(context, () => _fetchService.TriggerReconciliationFetch(fetchRequest));
            return success ? new StatusCodeResult(StatusCodes.Status202Accepted) : new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        private async Task<ReconciliationFetchRequest> TryReadFetchRequest()
        {
            using (var stream = new StreamReader(HttpContext.Request.Body))
            {
                var streamContents = await stream.ReadToEndAsync();
                return JsonConvert.DeserializeObject<ReconciliationFetchRequest>(streamContents);
            }
        }

        private bool IsFetchRequestValid(ReconciliationFetchRequest request)
        {
            var validationResults = new ReconciliationFetchRequestValidator().Validate(request);
            if (!validationResults.IsValid)
            {
                var metadata = new Dictionary<string, string> { { "ValidationErrors", GetValidationResultErrorMessage(validationResults) }, { "PACode", request.PACode } };
                _logger.LogInformationWithMetadata("Reconciliation fetch request failed validation.", metadata);
            }
            return validationResults.IsValid;
        }

        private string GetValidationResultErrorMessage(ValidationResult validationResults)
        {
            var validationErrorMessages = validationResults.Errors.Select(x => x.ErrorMessage);
            return string.Join(", ", validationErrorMessages);
        }
    }
}
