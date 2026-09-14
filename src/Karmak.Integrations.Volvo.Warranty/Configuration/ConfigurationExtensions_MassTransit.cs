using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using Karmak.Integrations.Volvo.Warranty.Consumers;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Karmak.Integrations.Volvo.Warranty.Configuration
{
    public static partial class ConfigurationExtensions
    {
        public static void AddWarrantyConsumers(this IBusRegistrationConfigurator configurator)
        {
            configurator.AddConsumer<RepairOrderSavedConsumer>()
                .Endpoint(e =>
                {
                    e.Name = "volvo.warranty.repairorders.v5";
                });

            configurator.AddConsumer<SubmitClaimConsumer>()
                .Endpoint(e =>
                {
                    e.Name = "volvo.warranty.submitclaim.v5";
                });

            configurator.AddConsumer<UpdateSnapshotConsumer>()
                .Endpoint(e =>
                 {
                     e.Name = "volvo.warranty.updatesnapshot.v5";
                 });

            configurator.AddConsumer<SubmitClaimFaultConsumer>();
            configurator.AddConsumer<UpdateSnapshotFaultConsumer>();
        }

        public static void ConfigureWarrantyConsumers(this IServiceBusBusFactoryConfigurator serviceBusConfig, IBusRegistrationContext context, IConfiguration config)
        {
            serviceBusConfig.Message<RepairOrderReceived>(t =>
            {
                //This is deliberately reading the react config value as this processing is driven by messages placed in this queue.
                t.SetEntityName(config["Volvo:React:RepairOrders:TopicName"]);
            });

            serviceBusConfig.Message<SubmitClaimPayload>(t =>
            {
                t.SetEntityName(config["Volvo:Warranty:SubmitClaim:TopicName"]);
            });

            serviceBusConfig.Message<UpdateSnapshotMessage>(t =>
            {
                t.SetEntityName(config["Volvo:Warranty:UpdateSnapshot:TopicName"]);
            });
        }
    }
}
