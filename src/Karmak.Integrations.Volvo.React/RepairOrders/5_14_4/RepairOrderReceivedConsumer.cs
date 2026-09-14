using Karmak.Integrations.Elk.Identity;
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
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Mappers.RepairOrder;
using Karmak.Integrations.Volvo.React.Persistence.RepairOrderHistory;
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
using System.Threading.Tasks;
using static Karmak.Integrations.Volvo.React.Transport.Soap.SoapResponseExtensions;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V5_14_4
{
    internal class RepairOrderReceivedConsumer : IConsumer<RepairOrderReceived>
    {
        private const string _volvoVersion = "UDB5.2";
        private readonly IVolvoExtendedLoggingService _extendedLoggingService;
        private readonly ICommentsService _commentsSvc;
        private readonly ILogger _logger;
        private readonly IVolvoEventHistoryProvider _volvoEventHistory;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ISoapRequestFactory _soapRequestFactory;
        private readonly IVolvoClient _volvoClient;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly IOptions<ProcessorOptions> _options;

        public RepairOrderReceivedConsumer(
            IVolvoExtendedLoggingService extendedLoggingService,
            ICommentsService commentsService,
            ILogger<RepairOrderReceivedConsumer> logger,
            IVolvoEventHistoryProvider volvoEventHistory,
            ISettingsProvider settingsProvider,
            ISoapRequestFactory soapRequestFactory,
            IVolvoClient volvoClient,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<ProcessorOptions> options)
        {
            _extendedLoggingService = extendedLoggingService;
            _commentsSvc = commentsService;
            _logger = logger;
            _volvoEventHistory = volvoEventHistory;
            _settingsProvider = settingsProvider;
            _soapRequestFactory = soapRequestFactory;
            _volvoClient = volvoClient;
            _claimCheckClient = claimCheckClient;
            _options = options;
        }

        public async Task Consume(ConsumeContext<RepairOrderReceived> context)
        {
            var update = await _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(context.Message.BlobName);

            int sendStatus = _options.Value.GetSendStatus(EntityTypes.RepairOrder, _volvoVersion, ImplicitElkContext.Current.ApplicationContext.Branch.ToString());
            if (sendStatus > 0)
            {
                await Execute(update);
            }
            else
            {
                _logger.LogInformationWithMetadata($"Repair Order {update.RepairOrderNumber} not processed for this version per configuration. UDB5.2", new Dictionary<string, string>(update.EntityMetadata()));
            }
        }

        private async Task Execute(RepairOrderSnapshot repairOrder)
        {
            await _extendedLoggingService.LogInboundDto(repairOrder, repairOrder.EntityMetadata());

            if (!repairOrder.IsValid(out var errorMessage))
            {
                throw new InvalidEntityException(typeof(RepairOrderSnapshot), errorMessage, new Dictionary<string, string>(repairOrder.EntityMetadata())
                {
                    [TelemetryKeys.Metric] = Metrics.InvalidEntity,
                    [TelemetryKeys.ErrorMessage] = errorMessage
                });
            }

            var settings = await GetValidatedVolvoSettingsAsync(repairOrder);

            var repairOrderHistory = await _volvoEventHistory.GetVolvoEventHistory(repairOrder.DealerInfo, repairOrder.RepairOrderNumber);

            if (repairOrder.SnapshotSequenceNumber < repairOrderHistory.LastKnownSnapshotSequenceNumber)
            {
                _logger.LogInformationWithMetadata("Received RepairOrderSnapshot out of order UDB5.2", new Dictionary<string, string>(repairOrder.EntityMetadata())
                {
                    [TelemetryKeys.Metric] = Metrics.RepairOrderSnapshotOutOfSequence,
                    [TelemetryKeys.LastKnownSnapshotSequenceNumber] = repairOrderHistory.LastKnownSnapshotSequenceNumber.ToString(),
                    [TelemetryKeys.OutOfOrderSequenceNumber] = repairOrder.SnapshotSequenceNumber.ToString()
                });
                return;
            }

            await SendCommentsToVolvo(repairOrder, settings);

            repairOrderHistory = await ProcessEventCollectionAsync(repairOrder, repairOrderHistory.GetOutboxEvents(repairOrder.SnapshotId), repairOrderHistory, settings);

            new VolvoEventEvaluator().AddPendingVolvoEventsToHistory(repairOrder, repairOrderHistory);

            bool forcingTransmission = false;
            if (repairOrderHistory.HasNoPendingEvents())
            {
                if (!repairOrder.ForceTransmission)
                {
                    _logger.LogInformationWithMetadata("Repair Order Snapshot did not trigger a state transition. UDB5.2", repairOrder.EntityMetadata());
                    return;
                }
                _logger.LogInformationWithMetadata($"Repair Order {repairOrder.RepairOrderNumber} did not trigger a state transition, but is marked to force transmission. UDB5.2", repairOrder.EntityMetadata());
                forcingTransmission = true;
            }

            repairOrderHistory = await CheckpointWorkAsync(repairOrder, repairOrderHistory, repairOrder.SnapshotId);

            if (forcingTransmission) // skip the process event collection and call straight to the transmit
                await TransmitEventToVolvoAsync(repairOrder, settings, repairOrderHistory.Events.FindLast(x => true));
            else
                repairOrderHistory = await ProcessEventCollectionAsync(repairOrder, repairOrderHistory.PendingEvents, repairOrderHistory, settings);

            var metaData = repairOrder.EntityMetadata();
            _logger.LogInformationWithMetadata($"Finished Processing Repair Order {repairOrder.RepairOrderNumber} UDB5.2", metaData);
        }

        private async Task<VolvoSettings> GetValidatedVolvoSettingsAsync(RepairOrderSnapshot repairOrder)
        {
            var settings = await _settingsProvider.GetSettingsAsync();
            var isValidSettings = settings.IsValid(out string errors);
            if (!isValidSettings)
            {
                _logger.LogInformationWithMetadata("Found Invalid Settings UDB5.2", new Dictionary<string, string>(repairOrder.EntityMetadata())
                {
                    [TelemetryKeys.Metric] = Metrics.InvalidSettings,
                    [TelemetryKeys.ErrorMessage] = errors
                });
                throw new InvalidSettingsException(errors);
            }
            return settings;
        }

        /// <summary>
        /// Sends pending events to Volvo, one by one, updating the repair order history at each step
        /// </summary>
        /// <param name="eventsToProcess"></param>
        /// <param name="repairOrderHistory">THIS IS TREATED AS A REF VARIABLE, BUT IF WE DIDN'T ALWAYS AWAIT THIS, WE WOULD BE F*CKED</param>
        /// <param name="settings"></param>
        /// <returns>AN UPDATED NEWLY PULLED HISTORY ITEM WITH A NEW ETAG</returns>
        private async Task<VolvoEventHistory> ProcessEventCollectionAsync(RepairOrderSnapshot repairOrder, IEnumerable<VolvoEvent> eventsToProcess, VolvoEventHistory repairOrderHistory, VolvoSettings settings)
        {
            foreach (var volvoEvent in eventsToProcess)
            {
                if (volvoEvent.TransmitToVolvo)
                {
                    await TransmitEventToVolvoAsync(repairOrder, settings, volvoEvent);
                    repairOrderHistory = await MarkEventAsTransmittedAsync(repairOrder, repairOrderHistory, volvoEvent);
                    // what if transmission fails?
                    // It throws and isn't caught, so we don't mark the event as transmitted. 
                }
            }
            return repairOrderHistory;

        }

        /// <summary>
        /// This thing acts like repairOrderHistory is a ref variable, but it is async, so lots of assholiness happened here
        /// So NOW it will act the way they thought it acted, but better practice is to explicitly take the return value
        /// </summary>
        /// <param name="repairOrderHistory">We do update this, but that is shaky AF with async</param>
        /// <param name="volvoEvent"></param>
        /// <returns>A NEWLY PULLED RECORD WITH AN UPDATED ETAG</returns>
        private async Task<VolvoEventHistory> MarkEventAsTransmittedAsync(RepairOrderSnapshot repairOrder, VolvoEventHistory repairOrderHistory, VolvoEvent volvoEvent)
        {
            repairOrderHistory.MarkEventAsTransmitted(volvoEvent);
            try
            {
                await _volvoEventHistory.UpsertVolvoEventHistory(repairOrderHistory);
            }
            catch (Exception e)
            {
                _logger.LogErrorWithMetadata("Failed to Upsert Volvo Event History Mark As Transmitted UDB5.2", e, repairOrder.EntityMetadata());
            }
            repairOrderHistory = await _volvoEventHistory.GetVolvoEventHistory(repairOrder.DealerInfo, repairOrder.RepairOrderNumber);
            return repairOrderHistory;
        }

        /// <summary>
        /// This was treated as though it updated the incoming history with a new ETAG.  IT DID NOT BUT DOES NOW
        /// </summary>
        /// <param name="history"></param>
        /// <param name="snapshotId"></param>
        /// <returns>A NEWLY PULLED RECORD WITH AN UPDATED ETAG</returns>
        private async Task<VolvoEventHistory> CheckpointWorkAsync(RepairOrderSnapshot repairOrder, VolvoEventHistory history, Guid snapshotId)
        {
            history.ApplySnapshotIdToAllPendingEvents(snapshotId);
            history.ApplyAllPendingEventsToJournalHistory();
            history.LastKnownSnapshotSequenceNumber = repairOrder.SnapshotSequenceNumber;
            var pendingEvents = history.PendingEvents;
            try
            {
                await _volvoEventHistory.UpsertVolvoEventHistory(history);
            }
            catch (Exception e)
            {
                _logger.LogErrorWithMetadata("Failed to Upsert Volvo Event History Checkpoint Work UDB5.2", e, repairOrder.EntityMetadata());
            }
            history = await _volvoEventHistory.GetVolvoEventHistory(repairOrder.DealerInfo, repairOrder.RepairOrderNumber);
            history.PendingEvents = pendingEvents;
            return history;
        }

        private async Task TransmitEventToVolvoAsync(RepairOrderSnapshot repairOrder, VolvoSettings settings, VolvoEvent volvoEvent)
        {
            if (volvoEvent == null)
            {
                var msg = $"Attempting to transmit Repair Order {repairOrder.RepairOrderNumber}" +
                    $" with null {nameof(VolvoEvent)}, cancelling transmission. UDB5.2";
                var dict = new Dictionary<string, string>(repairOrder.EntityMetadata());
                _logger.LogInformationWithMetadata(msg, dict);
                throw new InvalidOperationException(msg);
            }

            _logger.LogInformationWithMetadata($"Repair Order {repairOrder.RepairOrderNumber} triggered: {volvoEvent.Status} UDB5.2", new Dictionary<string, string>(repairOrder.EntityMetadata())
            {
                [TelemetryKeys.Metric] = Metrics.RepairOrderHitTrigger,
                [TelemetryKeys.RepairOrderStatus] = volvoEvent.Status,
            });

            ProcessRepairOrderType processRepairOrder = null;
            if (repairOrder.ForceTransmission)
                processRepairOrder = CreateProcessRepairOrderTransmission.Historical(_options.Value, settings, repairOrder, volvoEvent).BuildMessage();
            else
                processRepairOrder = CreateProcessRepairOrderTransmission.New(_options.Value, settings, repairOrder, volvoEvent).BuildMessage();

            await _extendedLoggingService.LogOutBoundMessage(processRepairOrder.ToXDocument().ToString(), repairOrder.EntityMetadata());

            var soapMessageAddress = new ProcessRepairOrderWebServiceAddress(settings.RegionSettings.CountryCode + settings.InterfaceOptions.PaCode);
            var request = _soapRequestFactory.CreatePutRequest(soapMessageAddress, processRepairOrder);

            var envelope = new VolvoSoapRequest(request);
            var meta = new Dictionary<string, string>(repairOrder.EntityMetadata())
            {
                [TelemetryKeys.ReactInterface] = EntityTypes.RepairOrder,
                [TelemetryKeys.RepairOrderStatus] = volvoEvent.Status,
            };

            (await _volvoClient.OAuthSendSoapAsync(envelope, meta)).ThrowIfNotSuccessful();
        }

        /// <summary>
        /// Send comments to Volvo, updating the comments repair order history as needed
        /// </summary>
        private async Task SendCommentsToVolvo(RepairOrderSnapshot repairOrder, VolvoSettings settings)
        {
            var commentsRepairOrder = RepairOrderSnapshotShallowCopy.GetShallowCopy(repairOrder);
            commentsRepairOrder.RepairOrderNumber += "Comments";
            var repairOrderHistory = await _volvoEventHistory.GetVolvoEventHistory(commentsRepairOrder.DealerInfo, commentsRepairOrder.RepairOrderNumber);

            _logger.LogInformationWithMetadata("Looking for comments... UDB5.2", commentsRepairOrder.EntityMetadata());
            ReceiveCommentsBody body = _commentsSvc.BuildCommentsBody(settings, commentsRepairOrder);

            string newHash = _commentsSvc.GetCommentsHash(body?.payload?.comments);
            if (repairOrderHistory.LastKnownSnapshotCommentsHash == newHash)
            {
                _logger.LogInformationWithMetadata("No changes were made to comments, not sending. UDB5.2", commentsRepairOrder.EntityMetadata());
                return;
            }

            try
            {
                _logger.LogInformationWithMetadata($"Attempting to transmit comments... UDB5.2", commentsRepairOrder.EntityMetadata());
                bool success = await _commentsSvc.SendCommentsToVolvoAsync(body, settings?.InterfaceOptions?.ReactEmailAddresses, commentsRepairOrder.EntityMetadata());
                if (success)
                {
                    _logger.LogInformationWithMetadata($"Comments successfully transmitted. UDB5.2", commentsRepairOrder.EntityMetadata());
                    repairOrderHistory.LastKnownSnapshotCommentsHash = newHash;
                    await _volvoEventHistory.UpsertVolvoEventHistory(repairOrderHistory);
                }
                else
                {
                    _logger.LogInformationWithMetadata($"Comments were not transmitted, see prior telemetry for details. UDB5.2", commentsRepairOrder.EntityMetadata());
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithMetadata($"An unhandled exception occurred while transmitting comments: UDB5.2", ex, commentsRepairOrder.EntityMetadata());
            }
        }
    }
}