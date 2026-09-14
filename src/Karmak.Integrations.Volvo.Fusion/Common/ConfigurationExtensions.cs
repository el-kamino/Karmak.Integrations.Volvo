using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.Common;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddCommonDispatchers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InterfaceOptionsDispatchOptions>(options =>
        {
            options.VolvoInterfaceOptionsRecordDefinitionId = configuration["Volvo:CommonDispatch:InterfaceSettings:VolvoInterfaceOptionsRecordDefinitionId"];
        });

        services.AddKeyedScoped<IRequestDispatcher, MessagingErrorDispatcher>(MessagingErrorDispatcher.EntityType);
        services.AddKeyedScoped<IRequestDispatcher, InterfaceOptionsDispatch>(InterfaceOptionsDispatch.EntityType);
        return services;
    }
}
