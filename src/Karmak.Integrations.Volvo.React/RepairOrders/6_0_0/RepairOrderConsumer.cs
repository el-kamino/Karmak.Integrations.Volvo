using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Exceptions;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Comments;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Extensions;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Validators.RepairOrders;
using Karmak.Integrations.Volvo.React.Validators.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0
{
    internal class RepairOrderConsumer : IConsumer<RepairOrderReceived>
    {
        private const string _volvoVersion = "UDB6.0";
        private readonly ICommentsService _commentsSvc;
        private readonly ILogger _logger;
        private readonly IVolvoRepairOrderStatusEvaluater _repairOrderStatusEvaluater;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IVolvoClient _volvoClient;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly IOptions<ProcessorOptions> _options;

        public RepairOrderConsumer(
            ICommentsService commentsService,
            ILogger<RepairOrderConsumer> logger,
            IVolvoRepairOrderStatusEvaluater repairOrderStatusEvaluater,
            ISettingsProvider settingsProvider,
            IVolvoClient volvoClient,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<ProcessorOptions> options)
        {
            _commentsSvc = commentsService;
            _logger = logger;
            _repairOrderStatusEvaluater = repairOrderStatusEvaluater;
            _settingsProvider = settingsProvider;
            _volvoClient = volvoClient;
            _claimCheckClient = claimCheckClient;
            _options = options;
        }

        public async Task Consume(ConsumeContext<RepairOrderReceived> context)
        {
            var update = await _claimCheckClient.RetrieveAsync<RepairOrderSnapshot>(context.Message.BlobName);
            LogReceived(update.RepairOrderNumber, context.Message.BlobName);

            int sendStatus = _options.Value.GetSendStatus(EntityTypes.RepairOrder, _volvoVersion, ImplicitElkContext.Current.ApplicationContext.Branch.ToString());
            if (sendStatus > 0)
            {
                await Execute(update, sendStatus);
            }
            else
            {
                LogNotSending(update.RepairOrderNumber);
            }
            _logger.LogInformation(LogMessages.FinishedProcessing, $"Repair Order {update.RepairOrderNumber}", _volvoVersion);
        }

        private async Task Execute(RepairOrderSnapshot repairOrder, int sendStatus)
        {
            if (!repairOrder.IsValid(out var errorMessage))
            {
                _logger.LogError(LogMessages.FailedValidation, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion, errorMessage);
                throw new InvalidEntityException(typeof(RepairOrderSnapshot), errorMessage, new Dictionary<string, string>(repairOrder.EntityMetadata())
                {
                    [TelemetryKeys.Metric] = Metrics.InvalidEntity,
                    [TelemetryKeys.ErrorMessage] = errorMessage
                });
            }

            var lastRepairOrderState = await _repairOrderStatusEvaluater.GetLastRoState(repairOrder);
            var settings = await GetValidatedVolvoSettingsAsync(repairOrder);
            var currentRepairOrderState = _repairOrderStatusEvaluater.GetCurrentRoState(repairOrder, lastRepairOrderState);

            //This will be removed at a future date (when Volvo says so) - comments are now included in the RO.
            //  We can then move settings retrieval (costly) into PayloadBuilder, no need to load them if we do not need to send a payload.
            currentRepairOrderState.CommentHash = await SendCommentsToVolvoAndReturnHash(repairOrder, lastRepairOrderState.CommentHash, settings);

            var shouldSend = _repairOrderStatusEvaluater.RepairOrderTriggersSend(currentRepairOrderState, lastRepairOrderState, repairOrder.ForceTransmission);

            if (!shouldSend)
                return;

            var payloadBuilder = new VolvoRepairOrderBuilder(settings, repairOrder, currentRepairOrderState, repairOrder.ForceTransmission);
            var payload = payloadBuilder.BuildPayload();
            if (payloadBuilder.HasNoTasksAndIsNotDeletedOrClosed(payload))
            {
                _logger.LogInformation(LogMessages.NoTasks, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
                return;
            }

            payloadBuilder.RemoveAllTasksIfNeeded(ref payload);
            var roPayloadJson = payloadBuilder.SerializeROPayload(payload);

            await SendPhantomPayloadsIfNeeded(roPayloadJson, payloadBuilder, currentRepairOrderState, repairOrder, sendStatus);

            _logger.LogInformation(LogMessages.RoProcessing, $"Repair Order {repairOrder.RepairOrderNumber}", settings.InterfaceOptions.PaCode, _volvoVersion);
            await SendPayloadAsync(roPayloadJson, repairOrder, sendStatus);
            if (currentRepairOrderState.RepairOrderStatusState.VolvoRoStatus == VolvoRepairOrderStatus.ARRIVED)
            {
                currentRepairOrderState.HasSentVehicleArrival = true;
            }

            await _repairOrderStatusEvaluater.SaveRepairOrderState(currentRepairOrderState);
        }

        private async Task SendPayloadAsync(string payload, RepairOrderSnapshot repairOrder, int sendStatus)
        {
            var meta = new Dictionary<string, string>(repairOrder.EntityMetadata())
            {
                [TelemetryKeys.ReactInterface] = EntityTypes.RepairOrder,
            };
            
            await _volvoClient.OAuth2_0SendJsonAsync(payload, sendStatus == (int)ProcessorOptions.SendStatus.Pilot, EntityTypes.RepairOrder, meta);
        }

        private async Task SendPhantomPayloadsIfNeeded(string roPayloadJson, VolvoRepairOrderBuilder payloadBuilder, VolvoRepairOrderState currentRepairOrderState, RepairOrderSnapshot repairOrder, int sendStatus)
        {
            if (_repairOrderStatusEvaluater.ShouldSendPhantomVehicleArrival(currentRepairOrderState))
            {
                _logger.LogInformation(LogMessages.SendingVehicleArrival, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
                var phantomPayload = payloadBuilder.BuildPhantomVehicleArrival(roPayloadJson);

                await SendPayloadAsync(phantomPayload, repairOrder, sendStatus);
                currentRepairOrderState.HasSentVehicleArrival = true;
            }
            if (_repairOrderStatusEvaluater.ShouldSendPhantomTechnicianAllocated(currentRepairOrderState))
            {
                _logger.LogInformation(LogMessages.SendingTechAllocated, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
                var phantomPayload = payloadBuilder.BuildPhantomTechnicianAllocated(roPayloadJson);

                await SendPayloadAsync(phantomPayload, repairOrder, sendStatus);
                currentRepairOrderState.HasSentTechnicianAllocated = true;
            }
        }

        private async Task<VolvoSettings> GetValidatedVolvoSettingsAsync(RepairOrderSnapshot repairOrder)
        {
            var settings = await _settingsProvider.GetSettingsAsync();
            var isValidSettings = settings.IsValid(out string errorMessage);
            if (!isValidSettings)
            {
                _logger.LogError(LogMessages.InvalidSettings, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion, errorMessage);
                throw new InvalidSettingsException(errorMessage);
            }
            return settings;
        }

        // <summary>
        // Send comments to Volvo, and return the hash for persistance
        // </summary>
        private async Task<string> SendCommentsToVolvoAndReturnHash(RepairOrderSnapshot repairOrder, string oldCommentHash, VolvoSettings settings)
        {
            ReceiveCommentsBody body = _commentsSvc.BuildCommentsBody(settings, repairOrder);

            string newCommentHash = _commentsSvc.GetCommentsHash(body?.payload?.comments);
            if (oldCommentHash == newCommentHash)
            {
                _logger.LogInformation(LogMessages.NoCommentChanges, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
                return oldCommentHash;
            }

            try
            {
                bool success = await _commentsSvc.SendCommentsToVolvoAsync(body, settings?.InterfaceOptions?.ReactEmailAddresses, repairOrder.EntityMetadata());
                if (success)
                {
                    _logger.LogInformation(LogMessages.CommentsTransmitted, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
                    return newCommentHash;
                }
                _logger.LogInformation(LogMessages.CommentsNotTransmitted, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogMessages.CommentsTransmitException, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion, ex.Message);
            }
            return oldCommentHash;
        }

        private void LogReceived(string roNumber, string blobName)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                [TelemetryKeys.BlobName] = blobName
            }))
            {
                _logger.LogInformation(LogMessages.ReceivedPayload, $"Repair Order {roNumber}", _volvoVersion);
            }
        }

        private void LogNotSending(string roNumber)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["Branch"] = ImplicitElkContext.Current.ApplicationContext.Branch.ToString()
            }))
            {
                _logger.LogInformation(LogMessages.VersionNotEnabled, $"Repair Order {roNumber}", _volvoVersion);
            }
        }
    }
}