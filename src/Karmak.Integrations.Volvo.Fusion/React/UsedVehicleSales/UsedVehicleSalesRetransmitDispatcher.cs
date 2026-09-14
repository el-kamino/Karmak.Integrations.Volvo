using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Extensions;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Messages;
using Karmak.Integrations.Volvo.React.ElkContextRetrieval;
using Karmak.Integrations.Volvo.React.Persistence.React;
using Karmak.Integrations.Volvo.React.Utils;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Fusion.React.UsedVehicleSales
{
    internal class UsedVehicleSalesRetransmitDispatcher : IUsedVehicleSalesRetransmitDispatcher
    {
        private readonly IReactRepositoryWrapper _reactStorage;
        private readonly ILogger _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IReactElkContextRetriever _elkContextRetriever;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly UsedVehicleSalesRetransmitDispatcherOptions _options;

        public UsedVehicleSalesRetransmitDispatcher(
            IReactRepositoryWrapper reactStorage,
            ILogger<UsedVehicleSalesRetransmitDispatcher> logger,
            ISendEndpointProvider sendEndpointProvider,
            IReactElkContextRetriever elkContextRetriever,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<UsedVehicleSalesRetransmitDispatcherOptions> options)
        {
            _reactStorage = reactStorage;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _elkContextRetriever = elkContextRetriever;
            _claimCheckClient = claimCheckClient;
            _options = options.Value;
        }

        public async Task<Guid> RetransmitUsedVehicleSalesAsync(string paCode, DateTime? beginDateTimeWindow, DateTime? endDateTimeWindow, string[] invoiceNumbers)
        {
            var correlationId = Guid.NewGuid();
            var criteria = new VehicleSalesOrderSearchCriteria
            {
                WindowStart = beginDateTimeWindow,
                WindowEnd = endDateTimeWindow?.EndOfDay(),
                InvoiceNumbers = invoiceNumbers,
            };

            _logger.LogInformationWithMetadata($"VehicleSalesOrder: Received Request To Schedule Redelivery on #{_options.QueueName}", new Dictionary<string, string>
            {
                [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                [TelemetryKeys.RetransmitWindowStart] = criteria.WindowStart.ToString(),
                [TelemetryKeys.RetransmitWindowEnd] = criteria.WindowEnd.ToString(),
            });

            if (!criteria.IsProperlyConstrained)
            {
                throw new InvalidOperationException("Redelivery Criteria Must Include Either Start/End Dates, Invoice Numbers, or both.");
            }

            List<VehicleSalesOrder> vehicleSalesOrders = await _reactStorage.QueryVehicleSalesOrdersAsync(paCode, criteria);

            if (vehicleSalesOrders.Count > 0)
            {
                ElkContext elkContext = await _elkContextRetriever.GetElkContextAsync(paCode);
                ISendEndpoint endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{_options.QueueName}"));

                for (int i = 0; i < vehicleSalesOrders.Count; i++)
                {
                    VehicleSalesOrder vehicleSalesOrder = vehicleSalesOrders[i];
                    _logger.LogInformationWithMetadata(
                        $"VehicleSalesOrder: Sending vehicle sales order redelivery to {_options.QueueName}",
                        new Dictionary<string, string>(vehicleSalesOrder.EntityMetadata())
                        {
                            [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                            [TelemetryKeys.Index] = i.ToString(),
                            [TelemetryKeys.Total] = vehicleSalesOrders.Count.ToString(),
                        });

                    string blobName = await _claimCheckClient.UploadAsync(vehicleSalesOrder);
                    var message = new RetransmitVehicleSales
                    {
                        BlobName = blobName,
                        RequestCorrelationGuid = correlationId,
                        Index = i,
                        Total = vehicleSalesOrders.Count
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
