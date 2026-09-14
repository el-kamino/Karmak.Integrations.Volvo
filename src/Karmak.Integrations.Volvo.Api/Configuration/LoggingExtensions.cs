using Azure.Core;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Volvo.Dcds.FileDownload;
using Karmak.Integrations.Volvo.Dcds.FileUpload;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using MassTransit.Logging;
using MassTransit.Monitoring;
using OpenTelemetry.Logs;

namespace Karmak.Integrations.Volvo.Api.Configuration
{
    public static class LoggingExtensions
    {
        public static IServiceCollection ConfigureLogging(this IServiceCollection services, IConfiguration configuration, TokenCredential credential)
        {
            var otelBuilder = services.AddOpenTelemetry();

            otelBuilder.WithMetrics(metrics =>
            {
                metrics.AddMeter(InstrumentationOptions.MeterName);

                foreach (string meter in Processor.MeterNames)
                {
                    metrics.AddMeter(meter);
                }

                foreach (string meter in SendFileProcessor.MeterNames)
                {
                    metrics.AddMeter(meter);
                }
            });

            otelBuilder.WithTracing(tracing =>
            {
                tracing.AddSource(DiagnosticHeaders.DefaultListenerName);
                tracing.AddSource(Processor.ActivitySourceName);
                tracing.AddSource(SendFileProcessor.ActivitySourceName);
            });

            services.Configure<OpenTelemetryLoggerOptions>(options =>
            {
                options.IncludeScopes = true;
            });

            otelBuilder.WithLogging();
            
            otelBuilder.UseAzureMonitor(options =>
            {
                options.ConnectionString = configuration["Volvo:ApplicationInsights:ConnectionString"];
            });

            string storageAccountName = configuration["Volvo:Storage:StorageAccountName"]!;
            services.Configure<AzureBlobStorageExtendedLoggingServiceOptions>(options =>
            {
                options.BlobUri = $"https://{storageAccountName}.blob.core.windows.net/";
                options.Container = configuration["Volvo:ExtendedLogging:AzureBlobContainer"];
                options.Credential = credential;
                options.CompressFiles = false;
            });

            services.AddSingleton<IExtendedLogNameGenerator, RandomGuidExtendedLogNameGenerator>();
            services.AddSingleton<IExtendedLogDirectoryGenerator, DateDirectoryTreeGenerator>();
            services.AddSingleton<IExtendedLoggingService, AzureBlobStorageExtendedLoggingService>();
            return services;
        }
    }
}
