using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Extensions;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Messages;
using Karmak.Integrations.Volvo.React.ElkContextRetrieval;
using Karmak.Integrations.Volvo.React.Persistence.React;
using Karmak.Integrations.Volvo.React.Utils;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Fusion.React.PartsSalesOrders
{
    internal class PartsSalesOrderRetransmitDispatcher : IPartsSalesOrderRetransmitDispatcher
    {
        private readonly IReactRepositoryWrapper _reactStorage;
        private readonly ILogger _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IReactElkContextRetriever _elkContextRetriever;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly PartsSalesOrderRetransmitDispatcherOptions _options;

        public PartsSalesOrderRetransmitDispatcher(
            IReactRepositoryWrapper reactStorage,
            ILogger<PartsSalesOrderDispatcher> logger,
            ISendEndpointProvider sendEndpointProvider,
            IReactElkContextRetriever elkContextRetriever,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<PartsSalesOrderRetransmitDispatcherOptions> options)
        {
            _reactStorage = reactStorage;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _elkContextRetriever = elkContextRetriever;
            _claimCheckClient = claimCheckClient;
            _options = options.Value;
        }

        public async Task<Guid> RetransmitPartsSalesOrderAsync(string paCode, DateTime? beginDateTimeWindow, DateTime? endDateTimeWindow, string[] invoiceNumbers)
        {
            var correlationId = Guid.NewGuid();
            var criteria = new PartsSalesOrderSearchCriteria
            {
                WindowStart = beginDateTimeWindow,
                WindowEnd = endDateTimeWindow?.EndOfDay(),
                InvoiceNumbers = invoiceNumbers,
            };

            _logger.LogInformationWithMetadata($"PartsSalesOrder: Received Request To Schedule Redelivery on #{_options.QueueName}", new Dictionary<string, string>
            {
                [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                [TelemetryKeys.RetransmitWindowStart] = criteria.WindowStart.ToString(),
                [TelemetryKeys.RetransmitWindowEnd] = criteria.WindowEnd.ToString(),
            });

            if (!criteria.IsProperlyConstrained)
            {
                throw new InvalidOperationException("Redelivery Criteria Must Include Either Start/End Dates, Invoice Numbers, or both.");
            }

            List<PartsSalesOrder> partsSalesOrders = await _reactStorage.QueryPartsSalesOrdersAsync(paCode, criteria);

            if (partsSalesOrders.Count > 0)
            {
                ElkContext elkContext = await _elkContextRetriever.GetElkContextAsync(paCode);
                ISendEndpoint endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{_options.QueueName}"));

                for (int i = 0; i < partsSalesOrders.Count; i++)
                {
                    PartsSalesOrder partsSalesOrder = partsSalesOrders[i];
                    _logger.LogInformationWithMetadata(
                        $"PartsSalesOrder: Sending parts sales order redelivery to {_options.QueueName}",
                        new Dictionary<string, string>(partsSalesOrder.EntityMetadata())
                        {
                            [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                            [TelemetryKeys.Index] = i.ToString(),
                            [TelemetryKeys.Total] = partsSalesOrders.Count.ToString(),
                        });

                    string blobName = await _claimCheckClient.UploadAsync(partsSalesOrder);
                    var message = new RetransmitPartsSalesOrder
                    {
                        BlobName = blobName,
                        RequestCorrelationGuid = correlationId,
                        Index = i,
                        Total = partsSalesOrders.Count
                    };

                    await ImplicitElkContext.WithCurrentAsync(elkContext, () =>
                    {
                        return endpoint.Send(message);
                    });
                }
            }
            else
            {
                _logger.LogInformationWithMetadata("No records matched retransmit criteria.", new Dictionary<string, string>()
                {
                    [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                });
            }

            return correlationId;
        }
    }
}
