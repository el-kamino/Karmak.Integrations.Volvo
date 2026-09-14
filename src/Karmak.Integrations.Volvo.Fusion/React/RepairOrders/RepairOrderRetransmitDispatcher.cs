using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Extensions;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.React.ElkContextRetrieval;
using Karmak.Integrations.Volvo.React.Persistence.React;
using Karmak.Integrations.Volvo.React.Utils;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Fusion.React.RepairOrders
{
    internal class RepairOrderRetransmitDispatcher : IRepairOrderRetransmitDispatcher
    {
        private readonly IReactRepositoryWrapper _reactStorage;
        private readonly ILogger _logger;
        private readonly IReactElkContextRetriever _elkContextRetriever;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly RepairOrderRetransmitDispatcherOptions _options;

        public RepairOrderRetransmitDispatcher(
            IReactRepositoryWrapper reactStorage,
            ILogger<RepairOrderRetransmitDispatcher> logger,
            IReactElkContextRetriever elkContextRetriever,
            ISendEndpointProvider sendEndpointProvider,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<RepairOrderRetransmitDispatcherOptions> options)
        {
            _reactStorage = reactStorage;
            _logger = logger;
            _elkContextRetriever = elkContextRetriever;
            _sendEndpointProvider = sendEndpointProvider;
            _claimCheckClient = claimCheckClient;
            _options = options.Value;
        }

        public async Task<Guid> RetransmitRepairOrdersAsync(string paCode, DateTime? beginDateTimeWindow, DateTime? endDateTimeWindow, string[] repairOrderNumbers)
        {
            var correlationId = Guid.NewGuid();
            var criteria = new RepairOrderSnapshotCriteria
            {
                WindowStart = beginDateTimeWindow,
                WindowEnd = endDateTimeWindow?.EndOfDay(),

                RepairOrderNumbers = repairOrderNumbers
                    .Where(ron => !string.IsNullOrWhiteSpace(ron))
                    .ToArray(),

                OnlyLastSnapshotPerRepairOrder = true,
            };

            if (!criteria.IsProperlyConstrained)
            {
                throw new InvalidOperationException("Redelivery Criteria Must Include Either Start/End Dates, Repair Order Numbers, or both.");
            }

            _logger.LogInformationWithMetadata($"Received Request To Schedule Redelivery on {_options.QueueName}",
               new Dictionary<string, string>
               {
                   [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                   [TelemetryKeys.RetransmitWindowStart] = criteria.WindowStart?.ToString(),
                   [TelemetryKeys.RetransmitWindowEnd] = criteria.WindowEnd?.ToString(),
                   [TelemetryKeys.EntityCount] = (criteria.RepairOrderNumbers?.Count().ToString() ?? "null"),
                   ["Dealer.PaCode"] = paCode
               });

            List<RepairOrderSnapshot> snapshots = await _reactStorage.QueryRepairOrdersAsync(paCode, criteria);

            _logger.LogInformationWithMetadata("Executed retrieval query",
                new Dictionary<string, string>
                {
                    [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                    [TelemetryKeys.EntityCount] = snapshots.Count.ToString()
                });

            if (snapshots.Count > 0)
            {
                ElkContext elkContext = await _elkContextRetriever.GetElkContextAsync(paCode);
                ISendEndpoint endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{_options.QueueName}"));

                for (int i = 0; i < snapshots.Count; i++)
                {
                    RepairOrderSnapshot snapshot = snapshots[i];

                    _logger.LogInformationWithMetadata(
                        $"Sending repair order snapshot redelivery to {_options.QueueName}",
                        new Dictionary<string, string>(snapshot.EntityMetadata())
                        {
                            [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                            [TelemetryKeys.Index] = i.ToString(),
                            [TelemetryKeys.Total] = snapshots.Count.ToString(),
                            [TelemetryKeys.RepairOrderIdentifier] = snapshot.RepairOrderID.ToString()
                        });

                    string blobName = await _claimCheckClient.UploadAsync(snapshot);
                    var message = new RetransmitRepairOrder
                    {
                        BlobName = blobName,
                        RequestCorrelationGuid = correlationId,
                        Index = i,
                        Total = snapshots.Count
                    };

                    await ImplicitElkContext.WithCurrentAsync(elkContext, () =>
                    {
                        return endpoint.Send(message);
                    });
                }
            }
            else
            {
                _logger.LogInformationWithMetadata(
                    "No records matched retransmit criteria.",
                    new Dictionary<string, string>()
                    {
                        [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                    });
            }

            return correlationId;
        }
    }
}
