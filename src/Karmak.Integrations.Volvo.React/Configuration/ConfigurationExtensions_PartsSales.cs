using Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.React.Configuration
{
    public static partial class ConfigurationExtensions
    {
        private static IServiceCollection AddPartsSalesProcessing(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IProcessPartsSalesOrder, ProcessPartsSalesOrder>();
            return services;
        }
    }
}
