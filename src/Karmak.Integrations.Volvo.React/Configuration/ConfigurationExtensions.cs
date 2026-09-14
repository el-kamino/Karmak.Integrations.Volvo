using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Retrieval;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.ElkContextRetrieval;
using Karmak.Integrations.Volvo.React.Persistence.React;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.Security;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using AzureCosmosClient = Microsoft.Azure.Cosmos.CosmosClient;

namespace Karmak.Integrations.Volvo.React.Configuration;

public static partial class ConfigurationExtensions
{
    public static IServiceCollection AddVolvoReactProcessing(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ProcessorOptions>(options =>
        {
            options.Environment = config["Volvo:React:Environment"];
            options.VersionOptions = config.GetValue<string>("Volvo:React:VersionOptions") is { } versionOptionsJson
                ? System.Text.Json.JsonSerializer.Deserialize<EntityVersionOptions>(versionOptionsJson)
                : null;
        });

        services.AddSingleton<ISoapRequestFactory, SoapRequestFactory>();
        services.AddSingleton<IVolvoExtendedLoggingService, VolvoExtendedLoggingService>();

        services.AddSingleton(new AzureCosmosClient(config["Volvo:React:Database:ConnectionString"]));
        services.AddVolvoClient(config);

        services.AddScoped<IReactRepositoryWrapper, ReactRepositoryWrapper>();

        services.AddRepairOrdersProcessing(config);
        services.AddPartsSalesProcessing(config);
        services.AddUsedVehicleSalesProcessing(config);
        services.AddCustomerUpdateProcessing(config);
        services.AddPartsInventoryReportProcessing(config);

        services.AddKeyedScoped<IContextClient, ContextClient>(
               ReactElkContextRetriever.ServiceKey,
               (sp, key) =>
               {
                   var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                   var cache = sp.GetRequiredService<HybridCache>();
                   var options = Options.Create(
                       new ContextClientOptions
                       {
                           ClientId = config["Volvo:React:ElkContext:ClientId"]!,
                           ClientSecret = config["Volvo:React:ElkContext:ClientSecret"]!,
                           Url = config["Volvo:React:ElkContext:Url"]!,
                           CacheExpiration = TimeSpan.FromMinutes(30)
                       });

                   return new ContextClient(http, cache, options);
               });

        services.AddTransient<IReactElkContextRetriever, ReactElkContextRetriever>();

        return services;
    }

    private static IServiceCollection AddVolvoClient(this IServiceCollection services, IConfiguration config)
    {
        if (config.GetValue<bool>("Volvo:React:TransmissionEnabled"))
        {
            services.AddKeyedSingleton<IOAuthClient>("React5Auth", (sp, key) =>
            {
                var options = new OAuthClientOptions
                {
                    ClientSecret = config["Volvo:React:OAuth:ClientSecret"],
                    ClientId = config["Volvo:React:OAuth:ClientId"],
                    TokenResource = config["Volvo:React:OAuth:Resource"],
                    TokenUri = config["Volvo:React:OAuth:TokenUri"]
                };

                return ActivatorUtilities.CreateInstance<OAuthClient>(sp, Options.Create(options));
            });

            services.AddKeyedSingleton<IOAuthClient>("React6Auth", (sp, key) =>
            {
                var options = new OAuthClientOptions
                {
                    ClientSecret = config["Volvo:React:OAuth20:ClientSecret"],
                    ClientId = config["Volvo:React:OAuth20:ClientId"],
                    Scope = config["Volvo:React:OAuth20:TokenScope"],
                    TokenUri = config["Volvo:React:OAuth20:TokenUri"]
                };

                return ActivatorUtilities.CreateInstance<OAuthClient>(sp, Options.Create(options));
            });

            services.AddKeyedSingleton<IOAuthClient>("React6PilotAuth", (sp, key) =>
            {
                var options = new OAuthClientOptions
                {
                    ClientSecret = config["Volvo:React:OAuth20:PilotClientSecret"],
                    ClientId = config["Volvo:React:OAuth20:PilotClientId"],
                    Scope = config["Volvo:React:OAuth20:PilotTokenScope"],
                    TokenUri = config["Volvo:React:OAuth20:TokenUri"]
                };

                return ActivatorUtilities.CreateInstance<OAuthClient>(sp, Options.Create(options));
            });

            services.AddScoped<IVolvoClient>(sp =>
            {
                string volvo60BaseUri = config["Volvo:React:Volvo60BaseEndpoint"];
                string volvo60PilotBaseUri = config["Volvo:React:Volvo60PilotBaseEndpoint"];
                string volvoUri = config["Volvo:React:VolvoEndpoint"];
                string roRoute60 = config["Volvo:React:RepairOrders:Volvo60Route"];

                Dictionary<string, string> routes60 = new Dictionary<string, string>
                {
                    {EntityTypes.RepairOrder, roRoute60}
                };

                HttpClient httpClient = sp
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient();

                IExtendedLoggingService extendedLoggingService = sp.GetRequiredService<IExtendedLoggingService>();
                ILogger logger = sp.GetRequiredService<ILogger<VolvoClient>>();
                IOAuthClient react5Auth = sp.GetRequiredKeyedService<IOAuthClient>("React5Auth");
                IOAuthClient react6Auth = sp.GetRequiredKeyedService<IOAuthClient>("React6Auth");
                IOAuthClient react6PilotAuth = sp.GetRequiredKeyedService<IOAuthClient>("React6PilotAuth");

                var volvoClientBuilder = VolvoClient.Builder()
                    .WithTelemetry(logger)
                    .WithHttpClient(httpClient)
                    .WithVolvoEnvironment(new Uri(volvoUri), volvo60BaseUri, volvo60PilotBaseUri, routes60)
                    .WithExtendedLogging(extendedLoggingService, Modules.INTEGRATIONS, "Volvo")
                    .WithVolvoOAuth(react5Auth)
                    .WithVolvoOAuth20(react6Auth, react6PilotAuth);

                return volvoClientBuilder.Build();
            });
        }
        else
        {
            services.AddSingleton<IVolvoClient>(sp =>
            {
                ILogger logger = sp.GetRequiredService<ILogger<NullVolvoClient>>();
                return new NullVolvoClient(logger);
            });
        }

        return services;
    }
}
