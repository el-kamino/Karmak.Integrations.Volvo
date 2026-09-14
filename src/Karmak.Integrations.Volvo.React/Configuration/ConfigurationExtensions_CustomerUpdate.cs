using Karmak.Integrations.Volvo.React.CustomerUpdate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.React.Configuration
{
    public static partial class ConfigurationExtensions
    {
        private static IServiceCollection AddCustomerUpdateProcessing(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IProcessCustomerUpdate, ProcessCustomerUpdate>();
            return services;
        }
    }
}
