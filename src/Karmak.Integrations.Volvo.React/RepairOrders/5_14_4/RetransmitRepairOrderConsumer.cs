using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Exceptions;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Comments;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Extensions;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Validators.RepairOrders;
using Karmak.Integrations.Volvo.React.Validators.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Karmak.Integrations.Volvo.React.Transport.Soap.SoapResponseExtensions;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V5_14_4
{
    internal sealed class RetransmitRepairOrderConsumer : IConsumer<RetransmitRepairOrder>
    {
        private readonly ICommentsService _commentsService;
        private readonly IVolvoExtendedLoggingService _extendedLoggingService;
        private readonly ILogger _logger;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ISoapRequestFactory _soapRequestFactory;
        private readonly IVolvoClient _volvoClient;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly IOptions<ProcessorOptions> _options;

        public RetransmitRepairOrderConsumer(
            IVolvoExtendedLoggingService extendedLoggingService,
            ICommentsService commentsService,
            ILogger<RetransmitRepairOrderConsumer> logger,
            ISettingsProvider settingsProvider,
            ISoapRequestFactory soapRequestFactory,
            IVolvoClient volvoClient,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<ProcessorOptions> options)
        {
            _extendedLoggingService = extendedLoggingService;
            _commentsService = commentsService;
            _logger = logger;
            _settingsProvider = settingsProvider;
            _soapRequestFactory = soapRequestFactory;
            _volvoClient = volvoClient;
            _claimCheckClient = claimCheckClient;
            _options = options;
        }

        public async Task Consume(ConsumeContext<RetransmitRepairOrder> context)
        {
            var update = await _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(context.Message.BlobName);
            await Execute(update, context.Message.RequestCorrelationGuid, context.Message.Index, context.Message.Total);
        }

        private async Task Execute(RepairOrderSnapshot repairOrder, Guid correlationId, int index, int total)
        {
            await _extendedLoggingService.LogInboundDto(repairOrder, repairOrder.EntityMetadata());

            var isValidRepairOrderDto = repairOrder.IsValid(out var errors);
            if (!isValidRepairOrderDto)
            {
                _logger.LogInformationWithMetadata("Found Invalid Repair Order Snapshot.", new Dictionary<string, string>(repairOrder.EntityMetadata())
                {
                    [TelemetryKeys.Metric] = Metrics.InvalidEntity,
                    [TelemetryKeys.ErrorMessage] = errors,
                    [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                    [TelemetryKeys.Index] = index.ToString(),
                    [TelemetryKeys.Total] = total.ToString()
                });
                throw new VolvoIntegrationServiceException(errors);
            }

            var settings = await GetValidatedVolvoSettingsAsync(repairOrder);

            await SendCommentsToVolvo(repairOrder, settings);

            var repairOrderHistory = new VolvoEventHistory();
            new VolvoEventEvaluator().AddPendingVolvoEventsToHistory(repairOrder, repairOrderHistory);

            if (repairOrderHistory.HasNoPendingEvents())
            {
                // are we sure we don't want to just resend the last one anyway?  // It starts with a blank history, so it will put everything possible in pending events.
                // We only get here if there isn't even an AT DEALERSHIP indicated.
                _logger.LogInformationWithMetadata("Repair Order Snapshot Resubmission did not trigger a state transition.", new Dictionary<string, string>(repairOrder.EntityMetadata())
                {
                    [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                    [TelemetryKeys.Index] = index.ToString(),
                    [TelemetryKeys.Total] = total.ToString()
                });
                return;
            }

            await TransmitEventToVolvoAsync(repairOrder, settings, repairOrderHistory.PendingEvents.Last()); // only send the final event

            _logger.LogInformationWithMetadata("Finished Processing Repair Order Snapshot retransmission.", new Dictionary<string, string>(repairOrder.EntityMetadata())
            {
                [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                [TelemetryKeys.Index] = index.ToString(),
                [TelemetryKeys.Total] = total.ToString()
            });
        }

        private async Task<VolvoSettings> GetValidatedVolvoSettingsAsync(RepairOrderSnapshot repairOrder)
        {
            var settings = await _settingsProvider.GetSettingsAsync();
            var isValidSettings = settings.IsValid(out string errors);
            if (!isValidSettings)
            {
                _logger.LogInformationWithMetadata("Found Invalid Settings", new Dictionary<string, string>(repairOrder.EntityMetadata())
                {
                    [TelemetryKeys.ErrorMessage] = errors,
                });

                throw new InvalidSettingsException(errors);
            }
            return settings;
        }

        private async Task TransmitEventToVolvoAsync(RepairOrderSnapshot repairOrder, VolvoSettings settings, VolvoEvent volvoEvent)
        {
            _logger.LogInformationWithMetadata($"Repair Order {repairOrder.RepairOrderNumber} triggered: {volvoEvent.Status}", repairOrder.EntityMetadata());
            var processRepairOrder = CreateProcessRepairOrderTransmission.Historical(_options.Value, settings, repairOrder, volvoEvent).BuildMessage();
            await _extendedLoggingService.LogOutBoundMessage(processRepairOrder.ToXDocument().ToString(), repairOrder.EntityMetadata());

            var soapMessageAddress = new ProcessRepairOrderWebServiceAddress(settings.RegionSettings.CountryCode + settings.InterfaceOptions.PaCode);
            var request = _soapRequestFactory.CreatePutRequest(soapMessageAddress, processRepairOrder);

            var requestEnvelope = new VolvoSoapRequest(request);
            var meta = new Dictionary<string, string>(repairOrder.EntityMetadata())
            {
                [TelemetryKeys.ReactInterface] = EntityTypes.RepairOrder
            };

            (await _volvoClient.OAuthSendSoapAsync(requestEnvelope, meta)).ThrowIfNotSuccessful();
        }

        private async Task SendCommentsToVolvo(RepairOrderSnapshot repairOrder, VolvoSettings settings)
        {
            _logger.LogInformationWithMetadata("Retransmit: Looking for comments...", repairOrder.EntityMetadata());
            try
            {
                _logger.LogInformationWithMetadata($"Attempting to retransmit comments...", repairOrder.EntityMetadata());
                ReceiveCommentsBody body = _commentsService.BuildCommentsBody(settings, repairOrder);

                bool success = await _commentsService.SendCommentsToVolvoAsync(body, settings?.InterfaceOptions?.ReactEmailAddresses, repairOrder.EntityMetadata());
                if (success)
                {
                    _logger.LogInformationWithMetadata($"Comments successfully retransmitted.", repairOrder.EntityMetadata());
                }
                else
                {
                    _logger.LogInformationWithMetadata($"Comments were not retransmitted, see prior telemetry for details.", repairOrder.EntityMetadata());
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithMetadata($"An unhandled exception occurred while retransmitting comments", ex, repairOrder.EntityMetadata());
            }
        }
    }
}
