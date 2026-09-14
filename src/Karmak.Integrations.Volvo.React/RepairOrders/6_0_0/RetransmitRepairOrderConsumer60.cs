using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Exceptions;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Comments;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Extensions;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Validators.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Karmak.Integrations.Volvo.React.ProcessorOptions;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0
{
    internal class RetransmitRepairOrderConsumer60 : IConsumer<RetransmitRepairOrder>
    {
        private const string _volvoVersion = "UDB6.0";
        private readonly ILogger _logger;
        private readonly IVolvoRepairOrderStatusEvaluater _repairOrderStatusEvaluater;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IVolvoClient _volvoClient;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly IOptions<ProcessorOptions> _options;

        public RetransmitRepairOrderConsumer60(
            ILogger<RepairOrderConsumer> logger,
            IVolvoRepairOrderStatusEvaluater repairOrderStatusEvaluater,
            ISettingsProvider settingsProvider,
            IVolvoClient volvoClient,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<ProcessorOptions> options)
        {
            _logger = logger;
            _repairOrderStatusEvaluater = repairOrderStatusEvaluater;
            _settingsProvider = settingsProvider;
            _volvoClient = volvoClient;
            _claimCheckClient = claimCheckClient;
            _options = options;
        }

        public async Task Consume(ConsumeContext<RetransmitRepairOrder> context)
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
            _logger.LogInformation(LogMessages.ForceTransmission, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);

            var lastRepairOrderState = await _repairOrderStatusEvaluater.GetLastRoState(repairOrder);
            var settings = await GetValidatedVolvoSettingsAsync(repairOrder);
            var isFirstSend = false;
            if (lastRepairOrderState.RepairOrderStatusState.VolvoRoStatus == VolvoRepairOrderStatus.UNDEFINED)
            {
                //this RO has not been sent via 6.0 yet, establish it's state so we can send the payload with the correct status.
                lastRepairOrderState = _repairOrderStatusEvaluater.GetCurrentRoState(repairOrder, lastRepairOrderState);
                isFirstSend = true;
            }
            if (lastRepairOrderState.RepairOrderStatusState.VolvoRoStatus == VolvoRepairOrderStatus.UNDEFINED)
            {
                //if it is still UNDEFINED, invalid RO state - log and return
                _logger.LogInformation(LogMessages.StatusUndefined, $"Repair Order {lastRepairOrderState.RepairOrderNumber}", _volvoVersion);
                return;
            }

            //set to force transmission, us the last RO State to build the Payload.
            var payloadBuilder = new VolvoRepairOrderBuilder(settings, repairOrder, lastRepairOrderState, true);  
            var payload = payloadBuilder.BuildPayload();

            payloadBuilder.RemoveAllTasksIfNeeded(ref payload);

            if (payloadBuilder.HasNoTasksAndIsNotDeletedOrClosed(payload) || (payloadBuilder.HasNoTasks(payload) && isFirstSend))
            {
                _logger.LogInformation(LogMessages.NoTasks, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
                return;
            }

            var roPayloadJson = payloadBuilder.SerializeROPayload(payload);
            await SendPhantomPayloadsIfNeeded(roPayloadJson, payloadBuilder, lastRepairOrderState, repairOrder, sendStatus);

            _logger.LogInformation(LogMessages.RoRetransmitting, $"Repair Order {repairOrder.RepairOrderNumber}", settings.InterfaceOptions.PaCode, _volvoVersion);
            await SendPayloadAsync(roPayloadJson, repairOrder, sendStatus);

            if (isFirstSend)
            {
                await _repairOrderStatusEvaluater.SaveRepairOrderState(lastRepairOrderState);
            }
        }

        private async Task SendPayloadAsync(string payload, RepairOrderSnapshot repairOrder, int sendStatus)
        {
            var meta = new Dictionary<string, string>(repairOrder.EntityMetadata())
            {
                [TelemetryKeys.ReactInterface] = EntityTypes.RepairOrder,
            };

            await _volvoClient.OAuth2_0SendJsonAsync(payload, sendStatus == (int)ProcessorOptions.SendStatus.Pilot, EntityTypes.RepairOrder, meta);
        }

        private async Task SendPhantomPayloadsIfNeeded(string roPayloadJson, VolvoRepairOrderBuilder payloadBuilder, VolvoRepairOrderState repairOrderState, RepairOrderSnapshot repairOrder, int sendStatus)
        {
            if (_repairOrderStatusEvaluater.ShouldSendPhantomPayloadsDuringRetransmit(repairOrderState))
            {
                _logger.LogInformation(LogMessages.SendingVehicleArrival, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
                var phantomPayload = payloadBuilder.BuildPhantomVehicleArrival(roPayloadJson);
                await SendPayloadAsync(phantomPayload, repairOrder, sendStatus);

                _logger.LogInformation(LogMessages.SendingTechAllocated, $"Repair Order {repairOrder.RepairOrderNumber}", _volvoVersion);
                phantomPayload = payloadBuilder.BuildPhantomTechnicianAllocated(roPayloadJson);
                await SendPayloadAsync(phantomPayload, repairOrder, sendStatus);
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

        private void LogReceived(string roNumber, string blobName)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                [TelemetryKeys.BlobName] = blobName
            }))
            {
                _logger.LogInformation(LogMessages.RetransmitReceivedPayload, $"Repair Order {roNumber}", _volvoVersion);
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