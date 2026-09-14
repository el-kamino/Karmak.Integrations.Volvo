using System;
using System.Net.Http;
using Azure.Core;
using Azure.Data.Tables;
using Elk.Core.ExtendedLogging;
using FluentValidation;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Retrieval;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Bridge;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.ElkContextRetrieval;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Persistence;
using Karmak.Integrations.Volvo.Warranty.Services;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.StandardCodes;
using Karmak.Integrations.Volvo.Warranty.Storage;
using Karmak.Integrations.Volvo.Warranty.Translators;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using Karmak.Integrations.Volvo.Warranty.Utilities;
using Karmak.Integrations.Volvo.Warranty.Validators;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Warranty.Configuration
{
    public static partial class ConfigurationExtensions
    {
        public static IServiceCollection AddVolvoWarrantyProcessing(this IServiceCollection services, IConfiguration config, TokenCredential credential)
        {
            services.Configure<WarrantyConfigurationOptions>(options =>
            {
                options.IsMockEnabled = config.GetValue<bool>("Volvo:Warranty:IsMockEnabled");
                options.IsSearchEnabled = config.GetValue<bool>("Volvo:Warranty:Search:Enabled");
            });

            services.AddSingleton<IRepairOrderToClaimMapper, RepairOrderToClaimMapper>();
            services.AddSingleton<IReconciliationToUpdateSnapshotMapper, ReconciliationToUpdateSnapshotMapper>();
            services.AddSingleton<IStatusToUpdateSnapshotMapper, StatusToUpdateSnapshotMapper>();
            services.AddSingleton(provider => SubmitClaimJobTranslatorFactory.Create());

            services.AddFusionRelay(config, credential);
            services.AddSearchServices(config);
            services.AddElkContextServices(config);
            services.AddStorageServices(config);
            services.AddStandardCodesClient(config);

            services.AddTransient<IValidator<WarrantyPaymentInformation>, WarrantyPaymentInformationValidator>();
            services.AddTransient<WarrantyRepairOrderValidator>();
            services.AddTransient<ReconciliationValidator>();
            services.AddTransient<StatusValidator>();
            services.AddTransient<IVolvoWarrantyReconcilliationFetchService, VolvoWarrantyReconcilliationFetchService>();
            services.AddTransient<IInboundMessageSender, MassTransitSender>();
            services.AddTransient<IEncryptionHandler, PassThroughEncryptionHandler>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddTransient<IDateTimeOffsetProvider, DateTimeProvider>();
            services.AddTransient<IClaimsService, ClaimsService>();
            services.AddTransient<IStandardCodesService, StandardCodesService>();
            services.AddTransient<IVolvoPushUpdateService, VolvoPushUpdateService>();
            services.AddTransient<IUpdateSnapshotCorrelationHandler, UpdateSnapshotToClaimCorrelationHandler>();

            services.AddTransient<ITranslatable<GetClaimReconciliationTranslatorArguments, GetServiceProcessingAdvisoryType>>(
                provider => new GetClaimReconciliationTranslator(config["Volvo:Warranty:VolvoEnvironment"], provider.GetService<IDateTimeProvider>()));

            services.AddTransient<ITranslatable<SubmitClaimTranslatorArguments, ProcessRepairOrderType>>(
                provider => new SubmitClaimTranslator(
                    provider.GetService<ITranslatable<SubmitClaimJobTranslatorArguments, JobExtended>>(),
                    config["Volvo:Warranty:VolvoEnvironment"]));

            services.AddSingleton<IExtendedLoggingClient>(sp =>
            {
                return new ExtendedLoggingClient(
                    sp.GetRequiredService<ILogger<ExtendedLoggingClient>>()
                    , sp.GetRequiredService<IExtendedLoggingService>(),
                    "Integrations", "VolvoWarranty");
            });

            services.AddTransient<IShowServiceProcessingAdvisoryHandler>(p =>
            {
                var endpoint = p.GetRequiredService<IInboundMessageSender>();
                var loggerFactory = p.GetRequiredService<ILoggerFactory>();

                return new ShowServiceProcessingAdvisoryHandler(
                    new ProcessClaimReconciliationHandler(
                        endpoint, p.GetRequiredService<IReconciliationToUpdateSnapshotMapper>(),
                        new PushValidationHandler(p.GetRequiredService<ReconciliationValidator>()), loggerFactory.CreateLogger<ProcessClaimReconciliationHandler>(),
                        p.GetService<IExtendedLoggingClient>(), p.GetRequiredService<IUpdateSnapshotCorrelationHandler>()),
                    new ProcessClaimStatusHandler(
                        endpoint, p.GetRequiredService<IStatusToUpdateSnapshotMapper>(),
                        new PushValidationHandler(p.GetRequiredService<StatusValidator>()), loggerFactory.CreateLogger<ProcessClaimStatusHandler>(),
                        p.GetService<IExtendedLoggingClient>(), p.GetRequiredService<IUpdateSnapshotCorrelationHandler>()),
                    new FaultHandler("Unknown message type", "500 bad request"));
            });

            return services;
        }

        private static IServiceCollection AddFusionRelay(this IServiceCollection services, IConfiguration config, TokenCredential credential)
        {
            string relayNamespace = config["Volvo:Bridge:RelayNamespace"];

            var builder = new RelayBuilder(relayNamespace, new DefaultAzureCredentialTokenProvider(credential));
            services.AddSingleton<IRelayBuilder>(builder);
            services.AddTransient<IFusionClient, FusionBridgeClient>();
            services.AddTransient<IFusionClient, FusionBridgeClient>();
            services.AddSingleton<IBridgeClient, BridgeClient>();
            return services;
        }

        private static IServiceCollection AddSearchServices(this IServiceCollection services, IConfiguration config)
        {
            // Searching the claims in sql, alongside the index-backed service above.
            services.AddTransient<IClaimSearchService, ClaimSearchService>();
            return services;
        }

        private static IServiceCollection AddElkContextServices(this IServiceCollection services, IConfiguration config)
        {
            // Elk Context Retrieval
            services.AddKeyedScoped<IContextClient, ContextClient>(
                WarrantyElkContextRetriever.ServiceKey,
                (sp, key) =>
                {
                    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                    var cache = sp.GetRequiredService<HybridCache>();
                    var options = Options.Create(
                        new ContextClientOptions
                        {
                            ClientId = config["Volvo:Warranty:ElkContext:ClientId"]!,
                            ClientSecret = config["Volvo:Warranty:ElkContext:ClientSecret"]!,
                            Url = config["Volvo:Warranty:ElkContext:Url"]!,
                            CacheExpiration = TimeSpan.FromMinutes(30)
                        });

                    return new ContextClient(http, cache, options);
                });

            services.AddTransient<IWarrantyElkContextRetriever, WarrantyElkContextRetriever>();
            return services;
        }

        private static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton(new TableServiceClient(config["Volvo:Storage:ConnectionString"]));
            services.AddSingleton<IWarrantyTableClient, WarrantyTableClient>();

            services.AddSingleton<IWarrantyClaimEntityMapper, WarrantyClaimEntityMapper>();
            services.AddSingleton<IClaimRepository, SqlClaimRepository>();

            return services;
        }

        private static IServiceCollection AddStandardCodesClient(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<StandardCodesClientOptions>(
               options =>
               {
                   options.CreatorNameCode = config["Volvo:Warranty:StandardCodes:CreatorNameCode"];
                   options.SenderNameCode = config["Volvo:Warranty:StandardCodes:SenderNameCode"];
                   options.VolvoEnvironment = config["Volvo:Warranty:VolvoEnvironment"];
               });

            services.Configure<StandardCodesBlobCacheOptions>(options =>
            {
                options.TimeToLiveHours = config.GetValue("Volvo:Warranty:StandardCodes:CacheTtlHours", 24);
            });

            services.AddScoped<IStandardCodesClient>(sp =>
            {
                var blobClient = sp.GetRequiredKeyedService<IKarmakBlobClient>("StandardCodes");
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var dateTimeProvider = sp.GetRequiredService<IDateTimeOffsetProvider>();
                var cacheOptions = sp.GetRequiredService<IOptions<StandardCodesBlobCacheOptions>>();
                var fallbackClient = new OWSStandardCodesClient(
                    sp.GetRequiredService<IVolvoClient>(),
                    sp.GetRequiredService<IDateTimeProvider>(),
                    loggerFactory.CreateLogger<OWSStandardCodesClient>(),
                    sp.GetRequiredService<IOptions<StandardCodesClientOptions>>());

                return new AzureBlobStorageStandardCodesClient(
                    blobClient, fallbackClient, loggerFactory.CreateLogger<AzureBlobStorageStandardCodesClient>(), dateTimeProvider, cacheOptions);
            });

            return services;
        }
    }
}
