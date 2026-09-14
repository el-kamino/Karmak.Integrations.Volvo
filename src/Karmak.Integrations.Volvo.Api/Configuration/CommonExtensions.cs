using Azure.Core;
using Karmak.ELK.Core.KICQ.Api.Core.Settings;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.Common.Sql.Migrations;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Api.Configuration
{
    public static class CommonExtensions
    {
        public static IServiceCollection ConfigureCommonServices(this IServiceCollection services, IConfiguration config, TokenCredential credential)
        {
            services.Configure<KarmakSettingsProviderOptions>(options =>
            {
                options.CacheTimeout = TimeSpan.FromMinutes(config.GetValue<int>("Volvo:Settings:CacheExpirationMinutes"));
                options.Credential = credential;
                options.SettingsServiceResourceId = config["Volvo:Settings:ResourceId"];
                options.Url = config["Volvo:Settings:Url"];
            });

            services.AddHttpClient<ISettingsProvider, KarmakSettingsProvider>();
            return services;
        }

        public static IServiceCollection ConfigureSqlDataLayer(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SqlDataLayerOptions>(options =>
            {
                options.ConnectionString = config["Volvo:Sql:ConnectionString"];
                options.CommandTimeoutSeconds = config.GetValue("Volvo:Sql:CommandTimeoutSeconds", options.CommandTimeoutSeconds);
                options.MigrationCommandTimeoutSeconds = config.GetValue("Volvo:Sql:MigrationCommandTimeoutSeconds", options.MigrationCommandTimeoutSeconds);
                options.MigrationLockTimeoutSeconds = config.GetValue("Volvo:Sql:MigrationLockTimeoutSeconds", options.MigrationLockTimeoutSeconds);
            });

            services.Configure<ReactDataCleanupOptions>(options =>
            {
                options.Enabled = config.GetValue("Volvo:Sql:Cleanup:Enabled", options.Enabled);
                options.RunAtUtc = config.GetValue("Volvo:Sql:Cleanup:RunAtUtc", options.RunAtUtc);
                options.RetentionDays = config.GetValue("Volvo:Sql:Cleanup:RetentionDays", options.RetentionDays);
                options.BatchSize = config.GetValue("Volvo:Sql:Cleanup:BatchSize", options.BatchSize);
            });

            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            services.AddSingleton<ISqlMigrationRunner, SqlMigrationRunner>();
            services.AddSingleton<IReactDataRepository, ReactDataRepository>();
            services.AddSingleton<IWarrantyDataRepository, WarrantyDataRepository>();

            return services;
        }

        public static IServiceCollection ConfigureStorage(this IServiceCollection services, IConfiguration config, TokenCredential credential)
        {
            services.Configure<ExternalBlobClientOptions>(options =>
            {
                options.ContainerName = config["Volvo:Storage:ExternalUploadContainerName"];
                options.BlobServiceUri = config["Volvo:Storage:BlobServiceUri"];
                options.Credential = credential;
            });

            services.AddSingleton<IExternalBlobClient, ExternalBlobClient>();

            services.AddKeyedSingleton<IKarmakBlobClient, KarmakBlobClient>("ClaimCheck", (sp, key) =>
            {
                var client = new KarmakBlobClient(
                    Options.Create(
                        new KarmakBlobClientOptions
                        {
                            ContainerName = config["Volvo:Storage:ClaimCheckContainerName"],
                            StorageConnectionString = config["Volvo:Storage:ConnectionString"]
                        }));

                return client;
            });

            services.AddKeyedSingleton<IKarmakBlobClient, KarmakBlobClient>("VolvoReact", (sp, key) =>
            {
                var client = new KarmakBlobClient(
                    Options.Create(
                        new KarmakBlobClientOptions
                        {
                            ContainerName = config["Volvo:Storage:ReactContainerName"],
                            StorageConnectionString = config["Volvo:Storage:ConnectionString"]
                        }));

                return client;
            });

            services.AddKeyedSingleton<IKarmakBlobClient, KarmakBlobClient>("StandardCodes", (sp, key) =>
            {
                var client = new KarmakBlobClient(
                    Options.Create(
                        new KarmakBlobClientOptions
                        {
                            ContainerName = config["Volvo:Storage:StandardCodesContainerName"],
                            StorageConnectionString = config["Volvo:Storage:ConnectionString"]
                        }));

                return client;
            });

            services.AddKeyedSingleton<IKarmakBlobClient, KarmakBlobClient>("SymptomCodes", (sp, key) =>
            {
                var client = new KarmakBlobClient(
                    Options.Create(
                        new KarmakBlobClientOptions
                        {
                            ContainerName = config["Volvo:Storage:SymptomCodesContainerName"],
                            StorageConnectionString = config["Volvo:Storage:ConnectionString"]
                        }));

                return client;
            });

            services.AddKeyedSingleton<IKarmakBlobClient, KarmakBlobClient>("InboxUIBlobClient", (sp, key) =>
            {
                var client = new KarmakBlobClient(
                    Options.Create(
                        new KarmakBlobClientOptions
                        {
                            ContainerName = config["Volvo:UI:Storage:InboxContainerName"],
                            StorageConnectionString = config["Volvo:UI:Storage:ConnectionString"]
                        }));

                return client;
            });

            services.AddKeyedSingleton<IKarmakBlobClient, KarmakBlobClient>("OasisUIBlobClient", (sp, key) =>
            {
                var client = new KarmakBlobClient(
                    Options.Create(
                        new KarmakBlobClientOptions
                        {
                            ContainerName = config["Volvo:UI:Storage:OasisContainerName"],
                            StorageConnectionString = config["Volvo:UI:Storage:ConnectionString"]
                        }));

                return client;
            });

            services.AddKeyedSingleton<IKarmakBlobClient, KarmakBlobClient>("WarrantyUIBlobClient", (sp, key) =>
            {
                var client = new KarmakBlobClient(
                    Options.Create(
                        new KarmakBlobClientOptions
                        {
                            ContainerName = config["Volvo:UI:Storage:WarrantyContainerName"],
                            StorageConnectionString = config["Volvo:UI:Storage:ConnectionString"]
                        }));

                return client;
            });

            return services;
        }
    }
}
