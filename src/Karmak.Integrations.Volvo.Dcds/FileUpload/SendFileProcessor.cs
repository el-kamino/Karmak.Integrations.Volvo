using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Volvo.Dcds.Api;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload
{
    public class SendFileProcessor : ISendFileProcessor
    {
        public const string ActivitySourceName = "integrations.volvo.dcds.processor.upload";

        private readonly IDcdsApiClient _apiClient;
        private readonly ILogger _logger;
        private readonly IExtendedLoggingService _extendedLoggingService;
        private readonly Counter<int> _filesProcessed;

        private static ActivitySource _activitySource = new ActivitySource(ActivitySourceName);

        public SendFileProcessor(
            IDcdsApiClient apiClient,
            ILogger<SendFileProcessor> logger,
            IExtendedLoggingService extendedLoggingService,
            IMeterFactory meterFactory)
        {
            _apiClient = apiClient;
            _logger = logger;
            _extendedLoggingService = extendedLoggingService;

            _filesProcessed = meterFactory
                .Create("integrations.volvo.dcds.processor.upload")
                .CreateCounter<int>(MeterNames[0]);
        }

        public static string[] MeterNames = ["integrations.volvo.dcds.processor.upload.files_processed"];

        public async Task ProcessAsync<TRequest>(TRequest sendRequest)
            where TRequest : ISendFileRequest
        {
            _logger.LogInformation("Volvo DCDS: SendFileProcessor Received Request.");

            using Activity? uploadActivity = _activitySource.StartActivity("dcds.upload", ActivityKind.Server);
            uploadActivity?.AddTag("dcds.file_type", sendRequest.SendType);
            uploadActivity?.AddTag("dcds.pa_code", sendRequest.DealerId);
            uploadActivity?.AddTag("dcds.file_name", sendRequest.FileName);

            if (string.IsNullOrWhiteSpace(sendRequest.DealerId))
            {
                throw new DcdsServiceValidationException("Volvo DCDS: DealerId is required to send a file to Volvo.");
            }

            try
            {
                _logger.LogInformation("Volvo DCDS: Sending File");

                await SavePutRequestMessageToExtendedLogging(sendRequest);
                var remoteFileId = await _apiClient.UploadFile(sendRequest);
                _filesProcessed.Add(1);

                _logger.LogInformation($"Volvo DCDS: Send File Complete; remote ID {remoteFileId}");

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Volvo DCDS: Error processing dcds upload request");
                throw new Exception("Volvo DCDS: Unexpected system error.", e);
            }
        }

        private async Task SavePutRequestMessageToExtendedLogging(ISendFileRequest request)
        {
            using var ms = new MemoryStream();
            JsonSerializer.Serialize(ms, request);

            var metadata = new Dictionary<string, string>
            {
                ["ContentDescription"] = "Volvo DCDS: Send file",
                ["SendType"] = request.SendType,
                ["SendVersion"] = request.SendTypeVersion
            };

            var log = new ExtendedLoggingRequest
            {
                Content = ms.ToArray(),
                Application = "Dcds",
                Module = "Integrations",
                Metadata = SanitizeMetadata(metadata)
            };

            await _extendedLoggingService.Execute(log);
        }

        private static IDictionary<string, string> SanitizeMetadata(IDictionary<string, string> metadata)
        {
            return metadata.ToDictionary(
                kv => Uri.EscapeDataString(kv.Key.Replace(".", "_").Replace('-', '_')),
                kv => Uri.EscapeDataString(kv.Value ?? string.Empty));
        }
    }
}