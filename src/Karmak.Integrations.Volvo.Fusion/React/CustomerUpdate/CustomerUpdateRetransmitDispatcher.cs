using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Extensions;
using Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Messages;
using Karmak.Integrations.Volvo.React.ElkContextRetrieval;
using Karmak.Integrations.Volvo.React.Persistence.React;
using Karmak.Integrations.Volvo.React.Utils;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactCustomerUpdate = Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate;

namespace Karmak.Integrations.Volvo.Fusion.React.CustomerUpdate
{
    internal class CustomerUpdateRetransmitDispatcher : ICustomerUpdateRetransmitDispatcher
    {
        private readonly IReactRepositoryWrapper _reactStorage;
        private readonly ILogger _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IReactElkContextRetriever _elkContextRetriever;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly CustomerUpdateRetransmitDispatcherOptions _options;

        public CustomerUpdateRetransmitDispatcher(
            IReactRepositoryWrapper reactStorage,
            ILogger<CustomerUpdateRetransmitDispatcher> logger,
            ISendEndpointProvider sendEndpointProvider,
            IReactElkContextRetriever elkContextRetriever,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<CustomerUpdateRetransmitDispatcherOptions> options)
        {
            _reactStorage = reactStorage;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _elkContextRetriever = elkContextRetriever;
            _claimCheckClient = claimCheckClient;
            _options = options.Value;
        }

        public async Task<Guid> RetransmitCustomerUpdateAsync(string paCode, DateTime? beginDateTimeWindow, DateTime? endDateTimeWindow, string[] volvoPassIds, string[] vins)
        {
            var correlationId = Guid.NewGuid();
            var criteria = new CustomerUpdateSearchCriteria
            {
                WindowStart = beginDateTimeWindow,
                WindowEnd = endDateTimeWindow?.EndOfDay(),
                VolvoPassIds = volvoPassIds,
                VINs = vins,
            };

            _logger.LogInformationWithMetadata(
                "CustomerUpdate: Received Request To Schedule Redelivery",
                new Dictionary<string, string>
                {
                    [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                    [TelemetryKeys.RetransmitWindowStart] = criteria.WindowStart.ToString(),
                    [TelemetryKeys.RetransmitWindowEnd] = criteria.WindowEnd.ToString(),
                });


            if (!criteria.IsDateConstrained)
            {
                throw new InvalidOperationException("Only date constraints are currently supported for customer update retransmit");
            }

            List<ReactCustomerUpdate> customerUpdates = await _reactStorage.QueryCustomerUpdatesAsync(paCode, criteria);

            if (customerUpdates.Count > 0)
            {
                ElkContext elkContext = await _elkContextRetriever.GetElkContextAsync(paCode);
                ISendEndpoint endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{_options.QueueName}"));

                for (int i = 0; i < customerUpdates.Count; i++)
                {
                    ReactCustomerUpdate customerUpdate = customerUpdates[i];
                    _logger.LogInformationWithMetadata(
                        $"CustomerUpdate: Sending customer update redelivery to {_options.QueueName}",
                        new Dictionary<string, string>(customerUpdate.EntityMetadata())
                        {
                            [TelemetryKeys.RetransmissionCorrelationGuid] = correlationId.ToString(),
                            [TelemetryKeys.Index] = i.ToString(),
                            [TelemetryKeys.Total] = customerUpdates.Count.ToString(),
                        });

                    string blobName = await _claimCheckClient.UploadAsync(customerUpdate);
                    var message = new RetransmitCustomerUpdate
                    {
                        BlobName = blobName,
                        RequestCorrelationGuid = correlationId,
                        Index = i,
                        Total = customerUpdates.Count
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
