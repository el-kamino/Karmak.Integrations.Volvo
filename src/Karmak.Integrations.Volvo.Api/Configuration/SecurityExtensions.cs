using Karmak.Integrations.Volvo.Api.Infrastructure.Authentication;
using Microsoft.Identity.Web;

namespace Karmak.Integrations.Volvo.Api.Configuration;

public static class SecurityExtensions
{
    internal const string MicrosoftScheme = "microsoft";
    internal const string VolvoCertScheme = "volvocert";

    public static IServiceCollection ConfigureSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<VolvoCertJwtBearerEvents>();
        services.Configure<VolvoCertJwtBearerEventOptions>(options => { options.AuthorityUrl = configuration["Volvo:KarmakId:Url"]!; });

        services.AddAuthentication(MicrosoftScheme)
              .AddJwtBearer(VolvoCertScheme, options =>
              {
                  options.Authority = configuration["Volvo:KarmakId:Url"];
                  options.Audience = configuration["Volvo:KarmakId:Audience"];
                  options.EventsType = typeof(VolvoCertJwtBearerEvents);
              })
              .AddMicrosoftIdentityWebApi(configuration, jwtBearerScheme: MicrosoftScheme, configSectionName: "Volvo:AzureAd");

        return services;
    }
}
