using Azure.Identity;
using Karmak.Integrations.Volvo.Common.MassTransit;
using Karmak.Integrations.Volvo.Dcds.Configuration;
using Karmak.Integrations.Volvo.React.Configuration;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;
using MassTransit;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Azure.Core;

namespace Karmak.Integrations.Volvo.Api.Configuration;

public static class MassTransitExtensions
{
    public static IServiceCollection ConfigureMassTransit(this IServiceCollection services, IConfiguration config, TokenCredential credential)
    {
        string uri = config["Volvo:ServiceBus:Uri"];
      
        services.AddMassTransit(mt =>
        {
            mt.DisableUsageTelemetry();

            mt.AddDcdsConsumers();
            mt.AddReactConsumers();
            mt.AddWarrantyConsumers();

            mt.UsingAzureServiceBus((context, serviceBusConfig) =>
            {
                serviceBusConfig.Host(uri, hostConfig =>
                {
                    hostConfig.TokenCredential = credential;
                });

                serviceBusConfig.UseImplicitElkContext();

                serviceBusConfig.Send<RepairOrderReceived>(c =>
                {
                    c.UseSessionIdFormatter(s => s.Message.RepairOrderId);
                });

                serviceBusConfig.ConfigureDcdsConsumers(context, config);
                serviceBusConfig.ConfigureReactConsumers(context, config);
                serviceBusConfig.ConfigureWarrantyConsumers(context, config);
                serviceBusConfig.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
