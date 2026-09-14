using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.React.PartsInventory
{
    internal static class ConfigurationExtensions
    {
        public static IServiceCollection AddPartsInventoryDispatch(this IServiceCollection services)
        {
            services.AddKeyedScoped<IRequestDispatcher, PartsInventoryDispatcher>(PartsInventoryDispatcher.EntityType);
            return services;
        }
    }
}
