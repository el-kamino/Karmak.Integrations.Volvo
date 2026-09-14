using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.React.UsedVehicleSales
{
    internal static class ConfigurationExtensions
    {
        public static IServiceCollection AddUsedVehicleSalesDispatch(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<UsedVehicleSalesRetransmitDispatcherOptions>(options =>
            {
                options.QueueName = config["Volvo:React:VehicleSales:RetransmitQueueName"];
            });

            services.AddKeyedScoped<IRequestDispatcher, UsedVehicleSalesDispatcher>(UsedVehicleSalesDispatcher.EntityType);
            services.AddScoped<IUsedVehicleSalesRetransmitDispatcher, UsedVehicleSalesRetransmitDispatcher>();

            return services;
        }
    }
}
