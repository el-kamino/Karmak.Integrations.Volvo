using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.React.PartsSalesOrders
{
    internal static class ConfigurationExtensions
    {
        public static IServiceCollection AddPartsSalesOrderDispatch(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<PartsSalesOrderRetransmitDispatcherOptions>(options =>
            {
                options.QueueName = config["Volvo:React:PartSales:RetransmitQueueName"];
            });

            services.AddKeyedScoped<IRequestDispatcher, PartsSalesOrderDispatcher>(PartsSalesOrderDispatcher.EntityType);
            services.AddScoped<IPartsSalesOrderRetransmitDispatcher, PartsSalesOrderRetransmitDispatcher>();

            return services;
        }
    }
}
