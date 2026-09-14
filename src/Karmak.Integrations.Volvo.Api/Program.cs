using Azure.Identity;
using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Api.Infrastructure.Background;
using Karmak.Integrations.Volvo.Api.Infrastructure.Middleware;
using Karmak.Integrations.Volvo.Api.Infrastructure.Startup;
using Karmak.Integrations.Volvo.Dcds.Configuration;
using Karmak.Integrations.Volvo.Fusion.Common;
using Karmak.Integrations.Volvo.Fusion.Dcds;
using Karmak.Integrations.Volvo.Fusion.React;
using Karmak.Integrations.Volvo.Inbox.Configuration;
using Karmak.Integrations.Volvo.Oasis.Configuration;
using Karmak.Integrations.Volvo.React.Configuration;
using Karmak.Integrations.Volvo.Warranty.Configuration;

namespace Karmak.Integrations.Volvo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var credential = new DefaultAzureCredential();

            if (builder.Environment.IsProduction())
            {
                builder.Configuration.AddAzureAppConfiguration(
                   options =>
                   {
                       options.Connect(new Uri(builder.Configuration["AppSettingsEndpoint"]!), credential)
                           .Select("Volvo:*");

                       options.ConfigureKeyVault(options =>
                       {
                           options.SetCredential(credential);
                       });
                   });
            }

            builder.Services.AddSingleton(TimeProvider.System);

            builder.Services.ConfigureSecurity(builder.Configuration);
            builder.Services.ConfigureLogging(builder.Configuration, credential);
            builder.Services.ConfigureMassTransit(builder.Configuration, credential);
            builder.Services.ConfigureStorage(builder.Configuration, credential);

            builder.Services.AddScoped<ImplicitElkContextMiddleware>();
            builder.Services.AddScoped<UICookieMiddleware>();

            builder.Services.ConfigureCommonServices(builder.Configuration, credential);
            builder.Services.ConfigureSqlDataLayer(builder.Configuration);
            builder.Services.AddVolvoReactDispatch(builder.Configuration);
            builder.Services.AddVolvoDcdsDispatch(builder.Configuration);
            builder.Services.AddCommonDispatchers(builder.Configuration);
            builder.Services.AddVolvoReactProcessing(builder.Configuration);
            builder.Services.AddVolvoDcdsServices(builder.Configuration);
            builder.Services.AddVolvoWarrantyProcessing(builder.Configuration, credential);
            builder.Services.AddOasisServices(builder.Configuration);
            builder.Services.AddInboxServices(builder.Configuration);
            builder.Services.AddHybridCache();
            builder.Services.AddControllers().AddNewtonsoftJson();

            builder.Services.AddHostedService<StorageSetupHostedService>();
            builder.Services.AddHostedService<SqlMigrationHostedService>();
            builder.Services.AddHostedService<ReactDataCleanupHostedService>();

            var app = builder.Build();

            app.UseHttpsRedirection();

            //This needs to run before authentication/authorization
            app.UseMiddleware<UICookieMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<ImplicitElkContextMiddleware>();

            app.MapControllers();
            app.Run();
        }
    }
}
