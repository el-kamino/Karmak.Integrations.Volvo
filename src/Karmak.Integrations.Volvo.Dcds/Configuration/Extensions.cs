using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Retrieval;
using Karmak.Integrations.Volvo.Dcds.Api;
using Karmak.Integrations.Volvo.Dcds.Auth;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using Karmak.Integrations.Volvo.Dcds.ElkContextRetrieval;
using Karmak.Integrations.Volvo.Dcds.FileDownload;
using Karmak.Integrations.Volvo.Dcds.FileUpload;
using Karmak.Integrations.Volvo.Dcds.FileUpload.FinancialStatement;
using Karmak.Integrations.Volvo.Dcds.FileUpload.PartsReturn;
using MassTransit;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Dcds.Configuration
{
    public static class Extensions
    {
        public static IServiceCollection AddVolvoDcdsServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddKeyedScoped<IContextClient, ContextClient>(
                DcdsElkContextRetriever.ServiceKey,
                (sp, key) =>
                {
                    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                    var cache = sp.GetRequiredService<HybridCache>();
                    var options = Options.Create(
                        new ContextClientOptions
                        {
                            ClientId = config["Volvo:Dcds:ElkContext:ClientId"]!,
                            ClientSecret = config["Volvo:Dcds:ElkContext:ClientSecret"]!,
                            Url = config["Volvo:Dcds:ElkContext:Url"]!,
                            CacheExpiration = TimeSpan.FromMinutes(30)
                        });

                    return new ContextClient(http, cache, options);
                });

            services.AddTransient<IDcdsElkContextRetriever, DcdsElkContextRetriever>();

            services.AddTransient<ISendFileProcessor, SendFileProcessor>();
            services.AddTransient<IFinancialStatementReceivedProcessor, FinancialStatementReceivedProcessor>();
            services.AddTransient<IPartsReturnProcessor, PartsReturnProcessor>();

            services.AddTransient<IDownloadFilesProcessor, Processor>();

            services.AddHttpClient<IDcdsApiClient, DcdsApiClient>();
            services.AddHttpClient<IOAuthTokenManager, OAuthTokenManager>();

            services.Configure<DcdsApiClientOptions>(options =>
            {
                options.InBoundApiUri = config["Volvo:Dcds:InBoundApiUri"]!;
                options.OutBoundApiUri = config["Volvo:Dcds:OutBoundApiUri"]!;
            });

            services.Configure<OAuthTokenManagerOptions>(options =>
            {
                options.ClientId = config["Volvo:Dcds:ClientId"]!;
                options.ClientSecret = config["Volvo:Dcds:ClientSecret"]!;
                options.InboundScope = config["Volvo:Dcds:InboundScope"]!;
                options.OutboundScope = config["Volvo:Dcds:OutboundScope"]!;
                options.TokenUri = config["Volvo:Dcds:TokenUri"]!;
            });

            return services;
        }

        public static void AddDcdsConsumers(this IBusRegistrationConfigurator configurator)
        {
            configurator.AddConsumer<PartsReturnConsumer>()
                .Endpoint(e =>
                 {
                     e.Name = "volvo.dcds.partsreturn";
                 });

            configurator.AddConsumer<FinancialStatementReceivedConsumer>()
                .Endpoint(e =>
                 {
                     e.Name = "volvo.dcds.financialstatement";
                 });
        }

        public static void ConfigureDcdsConsumers(this IServiceBusBusFactoryConfigurator serviceBusConfig, IBusRegistrationContext context, IConfiguration config)
        {
            serviceBusConfig.Message<FinancialStatementRequestReceived>(t =>
            {
                t.SetEntityName(config["Volvo:Dcds:FinancialStatements:TopicName"]!);
            });

            serviceBusConfig.Message<PartsReturnRequestReceived>(t =>
            {
                t.SetEntityName(config["Volvo:Dcds:PartsReturn:TopicName"]!);
            });
        }
    }
}
