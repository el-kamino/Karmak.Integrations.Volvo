using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.Dcds
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddVolvoDcdsDispatch(this IServiceCollection services, IConfiguration config)
        {
            services.AddAutoMapper(typeof(PartsReturnRequestMappingProfile));

            services.AddKeyedScoped<IRequestDispatcher, FinancialStatementDispatcher>(FinancialStatementDispatcher.EntityType);
            services.AddKeyedScoped<IRequestDispatcher, PartsReturnDispatcher>(PartsReturnDispatcher.EntityType);

            return services;
        }
    }
}
