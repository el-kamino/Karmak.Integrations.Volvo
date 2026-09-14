using Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Messages;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Messages;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Messages;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Messages;
using Karmak.Integrations.Volvo.React.Core.PartsInventory.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.CustomerUpdate;
using Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4;
using Karmak.Integrations.Volvo.React.UsedVehicleSales.V5_14_4;
using MassTransit;
using Microsoft.Extensions.Configuration;
using System;

namespace Karmak.Integrations.Volvo.React.Configuration
{
    public static partial class ConfigurationExtensions
    {
        public static void AddReactConsumers(this IBusRegistrationConfigurator configurator)
        {
            //These are pub/sub. Explicitly set the subscription name.
            configurator.AddConsumer<RepairOrderReceivedConsumer>()
            .Endpoint(e =>
            {
                e.Name = "volvo.react.repairorders.v5";
            });

            //This is the v6 consumer
            configurator.AddConsumer<RepairOrderConsumer>();

            configurator.AddConsumer<PartsSalesOrderConsumer>()
                .Endpoint(e =>
                {
                    e.Name = "volvo.react.partssales.v5";
                });

            configurator.AddConsumer<VehicleSalesOrderReceivedConsumer>()
                .Endpoint(e =>
                 {
                     e.Name = "volvo.react.vehiclesales.v5";
                 });

            configurator.AddConsumer<ProcessPartsInventoryReport>()
                .Endpoint(e =>
                 {
                     e.Name = "volvo.react.partsinventory.v5";
                 });

            configurator.AddConsumer<CustomerUpdateReceivedConsumer>()
                .Endpoint(e =>
                {
                    e.Name = "volvo.react.customerupdates.v5";
                });

            //Retransmit is queue based so the name is set on the entity below
            configurator.AddConsumer<RetransmitRepairOrderConsumer>();
            configurator.AddConsumer<RetransmitPartsSalesOrderConsumer>();
            configurator.AddConsumer<RetransmitVehicleSaleConsumer>();
            configurator.AddConsumer<RetransmitCustomerUpdateConsumer>();

            //This is the v6 consumer
            configurator.AddConsumer<RetransmitRepairOrderConsumer60>();
        }

        public static void ConfigureReactConsumers(this IServiceBusBusFactoryConfigurator serviceBusConfig, IBusRegistrationContext context, IConfiguration config)
        {
            serviceBusConfig.Message<RepairOrderReceived>(t =>
            {
                t.SetEntityName(config["Volvo:React:RepairOrders:TopicName"]);
            });

            serviceBusConfig.SubscriptionEndpoint<RepairOrderReceived>(
                "volvo.react.repairorders.v6",
                e =>
                {
                    e.RequiresSession = true;
                    e.MaxConcurrentSessions = 4;
                    e.MaxConcurrentCallsPerSession = 1;
                    e.SessionIdleTimeout = TimeSpan.FromSeconds(9);

                    e.ConfigureConsumer<RepairOrderConsumer>(context);
                });

            serviceBusConfig.Message<PartsSalesOrderReceived>(t =>
            {
                t.SetEntityName(config["Volvo:React:PartSales:TopicName"]);
            });

            serviceBusConfig.Message<VehicleSalesOrderReceived>(t =>
            {
                t.SetEntityName(config["Volvo:React:VehicleSales:TopicName"]);
            });

            serviceBusConfig.Message<PartsInventoryReportReceived>(t =>
            {
                t.SetEntityName(config["Volvo:React:PartsInventory:TopicName"]);
            });

            serviceBusConfig.Message<CustomerUpdateReceived>(t =>
            {
                t.SetEntityName(config["Volvo:React:CustomerUpdates:TopicName"]);
            });

            serviceBusConfig.ReceiveEndpoint(config["Volvo:React:RepairOrders:RetransmitQueueName"], endpoint =>
            {
                endpoint.ConfigureConsumer<RetransmitRepairOrderConsumer>(context);
                //This is the v6 consumer
                endpoint.ConfigureConsumer<RetransmitRepairOrderConsumer60>(context);
            });

            serviceBusConfig.ReceiveEndpoint(config["Volvo:React:PartSales:RetransmitQueueName"], endpoint =>
            {
                endpoint.ConfigureConsumer<RetransmitPartsSalesOrderConsumer>(context);
            });

            serviceBusConfig.ReceiveEndpoint(config["Volvo:React:VehicleSales:RetransmitQueueName"], endpoint =>
            {
                endpoint.ConfigureConsumer<RetransmitVehicleSaleConsumer>(context);
            });

            serviceBusConfig.ReceiveEndpoint(config["Volvo:React:CustomerUpdates:RetransmitQueueName"], endpoint =>
            {
                endpoint.ConfigureConsumer<RetransmitCustomerUpdateConsumer>(context);
            });
        }
    }
}
