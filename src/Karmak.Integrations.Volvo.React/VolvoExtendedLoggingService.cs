using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React
{
    public class VolvoExtendedLoggingService : IVolvoExtendedLoggingService
    {
        private const string ApplicationName = "Volvo";

        private readonly IExtendedLoggingService _extendedLoggingService;
        private readonly ILogger _logger;

        public VolvoExtendedLoggingService(IExtendedLoggingService extendedLoggingService, ILogger<VolvoExtendedLoggingService> logger)
        {
            _extendedLoggingService = extendedLoggingService;
            _logger = logger;
        }

        public async Task LogInboundDto(object entity, Dictionary<string, string> entityMetadata = null)
        {
            var request = new ExtendedLoggingRequest
            {
                Module = Modules.INTEGRATIONS,
                Application = ApplicationName,
                Content = GetSerializedPayload(entity)
            };

            var completeMetadata = entityMetadata ?? new Dictionary<string, string>();

            try
            {
                _logger.LogInformationWithMetadata("Uploading inbound DTO to extended logging.", completeMetadata);
                var blobReference = await _extendedLoggingService.Execute(request);
                _logger.LogInformationWithMetadata("Finished uploading inbound DTO to extended logging.", new Dictionary<string, string>(completeMetadata)
                {
                    [TelemetryKeys.BlobName] = blobReference.Name,
                    [TelemetryKeys.BlobUri] = blobReference.Uri?.AbsoluteUri
                });
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithMetadata("Exception thrown while uploading inbound DTO to extended logging.", ex, completeMetadata);
                throw;
            }
        }

        public async Task LogOutBoundMessage(string message, Dictionary<string, string> entityMetadata = null)
        {
            var request = new ExtendedLoggingRequest
            {
                Module = Modules.INTEGRATIONS,
                Application = ApplicationName,
                Content = Encoding.UTF8.GetBytes(message)
            };

            var completeMetadata = entityMetadata ?? new Dictionary<string, string>();
            try
            {
                _logger.LogInformationWithMetadata("Uploading outbound message to extended logging.", completeMetadata);
                var blobReference = await _extendedLoggingService.Execute(request);
                _logger.LogInformationWithMetadata("Finished uploading outbound message to extended logging.", new Dictionary<string, string>(completeMetadata)
                {
                    [TelemetryKeys.BlobName] = blobReference.Name,
                    [TelemetryKeys.BlobUri] = blobReference.Uri?.AbsoluteUri
                });
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithMetadata("Exception thrown while uploading outbound message to extended logging.", ex, completeMetadata);
                throw;
            }
        }

        public async Task LogOutBoundMessages(string[] messages, Dictionary<string, string> entityMetadata = null)
        {
            _logger.LogInformationWithMetadata($"Uploading {messages.Length} outbound message(s) to extended logging.", entityMetadata);

            foreach (string msg in messages)
            {
                await LogOutBoundMessage(msg, entityMetadata);
            }
        }

        private byte[] GetSerializedPayload(object entity)
        {
            using(var ms = new MemoryStream())
            using(var sw = new StreamWriter(ms))
            using(var jtw = new JsonTextWriter(sw))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(jtw, entity);
                return ms.ToArray();
            }
        }
    }
}
