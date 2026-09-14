using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.React.CustomerUpdate
{
    internal static class ConfigurationExtensions
    {
        public static IServiceCollection AddCustomerUpdateDispatch(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CustomerUpdateRetransmitDispatcherOptions>(options =>
            {
                options.QueueName = config["Volvo:React:CustomerUpdates:RetransmitQueueName"];
            });

            services.AddKeyedScoped<IRequestDispatcher, CustomerUpdateDispatcher>(CustomerUpdateDispatcher.EntityType);
            services.AddScoped<ICustomerUpdateRetransmitDispatcher, CustomerUpdateRetransmitDispatcher>();

            return services;
        }
    }
}
