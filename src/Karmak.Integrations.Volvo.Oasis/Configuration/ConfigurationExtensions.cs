using Karmak.Integrations.Volvo.Oasis.Mapping;
using Karmak.Integrations.Volvo.Oasis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Oasis.Configuration
{
    public static partial class ConfigurationExtensions
    {
        public static IServiceCollection AddOasisServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SymptomCodeProviderOptions>(options =>
            {
                options.BlobPath = config["Volvo:Oasis:SymptomCodesBlobPath"];
            });

            services.Configure<OasisDataProviderOptions>(options =>
            {
                options.ApplicationId = config["Volvo:Oasis:Client:ApplicationId"];
                options.ImsRegion = config["Volvo:Oasis:Client:ImsRegion"];
                options.Uri = config["Volvo:Oasis:Client:Uri"];
                options.UserId = config["Volvo:Oasis:Client:UserId"];
            });

            services.AddSingleton<IOasisRequestMapper, OasisRequestMapper>();
            services.AddTransient<ISymptomCodeProvider, SymptomCodeProvider>();
            services.AddScoped<IOasisDataProvider, OasisDataProvider>();

            if (config.GetValue<bool>("Volvo:Oasis:IsEnabled"))
            {
                services.AddScoped<IOasisClient, OasisClient>();
            }
            else
            {
                services.AddSingleton<IOasisClient, NullOasisClient>();
            }

            return services;
        }
    }
}
