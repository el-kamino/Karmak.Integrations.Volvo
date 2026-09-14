using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Translators;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Warranty.Consumers
{
    public class SubmitClaimConsumer : IConsumer<SubmitClaimPayload>
    {
        private readonly ILogger<SubmitClaimConsumer> _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly ITranslatable<SubmitClaimTranslatorArguments, ProcessRepairOrderType> _translator;
        private readonly ISettingsProvider _settingsClient;
        private readonly IVolvoClient _volvoClient;
        private readonly IClaimsService _claimsService;

        private readonly VolvoTransportConfigurationOptions _options;

        public SubmitClaimConsumer(
                ILogger<SubmitClaimConsumer> logger,
                IExtendedLoggingClient extendedLoggingClient,
                ITranslatable<SubmitClaimTranslatorArguments, ProcessRepairOrderType> translator,
                ISettingsProvider settingsClient,
                IVolvoClient volvoClient,
                IClaimsService claimsService,
                IOptions<VolvoTransportConfigurationOptions> options)
        {
            _logger = logger;
            _extendedLoggingClient = extendedLoggingClient;
            _translator = translator;
            _settingsClient = settingsClient;
            _volvoClient = volvoClient;
            _claimsService = claimsService;
            _options = options.Value;
        }

        public async Task Consume(ConsumeContext<SubmitClaimPayload> context)
        {
            var claims = context.Message.Claims;

            var claimMetadata = new Dictionary<string, string> {
                { "Correlation.Id", claims.First().CorrelationId },
                { "RepairOrder.Identifier", claims.First().RepairOrder.Identifier },
                { "Original.RepairOrder.Number", claims.First().RepairOrder.Identifier },
                { "RepairOrder.Number", claims.First().RepairOrder.SecondaryIdentifier },
                { "RepairOrder.SecondaryIdentifier", claims.First().RepairOrder.SecondaryIdentifier }
            };
            for (int i = 0; i < claims.Count(); i++)
            {
                claimMetadata.Add($"Claim_{i + 1}.Id", claims.ElementAt(i).Id);
            }

            _logger.LogInformationWithMetadata($"Processing {nameof(SubmitClaimPayload)} message.", claimMetadata.Concat(new Dictionary<string, string> {
                { "Redelivery.Attempt", context.GetRedeliveryCount().ToString() }
            }).ToDictionary(x => x.Key, x => x.Value));

            await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(context.Message), "Incoming Claim", VolvoLogContentType.JSON);

            var settings = await _settingsClient.GetSettingsAsync();
            var translation = Translate(claims, settings);

            var request = new SoapRequestFactory().CreatePutRequest(
                    new SoapMessageAddress
                    {
                        To = ConstantSettings.TransmitWarrantyClaimNamespace,
                        Action = ConstantSettings.StarPutMessageAction,
                        TargetService = ConstantSettings.VolvoProcessRepairOrderService,
                        TargetServiceVersion = ConstantSettings.VolvoOneWarrantySystemVersion,
                        SiteCode = settings.RegionSettings.CountryCode + settings.InterfaceOptions.PaCode,
                        RespondTo = _options.CallbackUri
                    },
                    translation);

            var result = await _volvoClient.OAuthSendSoapAsync(new VolvoSoapRequest(request), new Dictionary<string, string>());

            _logger.LogInformationWithMetadata($"Claim submitted to Volvo.", claimMetadata);

            switch (result)
            {
                case SoapResult.Success _:
                    _logger.LogInformationWithMetadata($"Claim submission succeeded.", claimMetadata.Concat(new Dictionary<string, string> {
                        { TelemetryKeys.DealerCode, settings.InterfaceOptions.PaCode },
                        { TelemetryKeys.KarmakAccountNumber, claims.First().Dealer?.Account?.Code},
                        { TelemetryKeys.OEM, TelemetryValues.Volvo },
                        { TelemetryKeys.BusinessProcess, "Claim Submitted" },
                        { TelemetryKeys.Module, TelemetryValues.Warranty },
                    }).ToDictionary(x => x.Key, x => x.Value));

                    foreach (var claim in claims)
                    {
                        await _claimsService.UpdateStatus(claim.Id, ClaimStatus.Submitted, FusionIdentity.SystemUser);
                    }

                    return;
                case SoapResult.Failure failure:
                    foreach (var claim in claims)
                    {
                        await _claimsService.UpdateStatus(claim.Id, ClaimStatus.Failed, FusionIdentity.SystemUser);
                    }
                    _logger.LogInformationWithMetadata($"Claim submission failed.", claimMetadata.Concat(new Dictionary<string, string> {
                        { TelemetryKeys.DealerCode, settings.InterfaceOptions.PaCode },
                        { TelemetryKeys.KarmakAccountNumber, claims.First().Dealer?.Account?.Code},
                        { TelemetryKeys.OEM, TelemetryValues.Volvo },
                        { TelemetryKeys.BusinessProcess, "Claim Submitted" },
                        { TelemetryKeys.Module, TelemetryValues.Warranty },
                        { "Error", failure.Response.InnerText }
                    }).ToDictionary(x => x.Key, x => x.Value));

                    throw new OWSClaimSubmissionException($"{nameof(SoapResult.Failure)} - Response status: {failure.HttpStatus}");
                case SoapResult.InvalidSignature invalidSignature:
                    foreach (var claim in claims)
                    {
                        await _claimsService.UpdateStatus(claim.Id, ClaimStatus.Failed, FusionIdentity.SystemUser);
                    }

                    throw new OWSClaimSubmissionException($"{nameof(SoapResult.InvalidSignature)} - Response: {invalidSignature.Response.OuterXml}");
                case SoapResult.Error error:
                    foreach (var claim in claims)
                    {
                        await _claimsService.UpdateStatus(claim.Id, ClaimStatus.Failed, FusionIdentity.SystemUser);
                    }

                    if (error.Response != null)
                    {
                        throw new OWSClaimSubmissionException($"{nameof(SoapResult.Error)} - Response: {error.Response}");
                    }

                    throw new OWSClaimSubmissionException($"{nameof(SoapResult.Error)} - Exception: {error.Exception?.Message}", error.Exception);
            }
        }

        private ProcessRepairOrderType Translate(IEnumerable<IClaim> claims, VolvoSettings settings)
        {
            try
            {
                _logger.LogInformation($"Translating {nameof(SubmitClaimPayload)} message.");
                return _translator.Translate(new SubmitClaimTranslatorArguments()
                {
                    Source = claims,
                    Settings = settings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate the message.");
                throw;
            }
        }
    }
}
