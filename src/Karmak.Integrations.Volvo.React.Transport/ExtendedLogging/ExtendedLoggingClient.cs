using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Volvo.Common.Logging;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Karmak.Integrations.Volvo.React.Transport.ExtendedLogging
{
    public class ExtendedLoggingClient : IExtendedLoggingClient
    {
        private readonly IExtendedLoggingService _extendedLoggingService;
        private readonly ILogger _logger;
        private readonly string _module;
        private readonly IApplicationResolver _resolver;

        public ExtendedLoggingClient(ILogger logger, IExtendedLoggingService extendedLoggingService, string module, string application)
        {
            _logger = logger;
            _extendedLoggingService = extendedLoggingService;
            _module = module;
            _resolver = new SimpleApplicationNameResolver(application);
        }

        public ExtendedLoggingClient(ILogger logger, IExtendedLoggingService extendedLoggingService, string module, IApplicationResolver resolver)
        {
            _logger = logger;
            _extendedLoggingService = extendedLoggingService;
            _module = module;
            _resolver = resolver;
        }

        public Task Execute(string content, string contentDescription)
        {
            return Execute<ExtendedLogReference>(
                content,
                contentDescription,
                new Dictionary<string, string>());
        }

        public async Task Execute(string content, string contentDescription, IDictionary<string, string> metadata)
        {
            await Execute<ExtendedLogReference>(content, contentDescription, metadata, null);
        }

        public async Task Execute(string content, string contentDescription, string contentType)
        {
            await Execute<ExtendedLogReference>(content, contentDescription, new Dictionary<string, string>(), contentType);
        }

        public async Task Execute(string content, string contentDescription, IDictionary<string, string> metadata, string contentType)
        {
            await Execute<ExtendedLogReference>(content, contentDescription, metadata, contentType); 
        }

        public async Task<T> Execute<T>(string content, string contentDescription)
            where T : ExtendedLogReference
        {
            return await Execute<T>(content, contentDescription, new Dictionary<string, string>());
        }

        public async Task<T> Execute<T>(string content, string contentDescription, IDictionary<string, string> metadata) 
            where T : ExtendedLogReference
        {
            return await Execute<T>(content, contentDescription, metadata, null);
        }

        public async Task<T> Execute<T>(string content, string contentDescription, string contentType) where T : ExtendedLogReference
        {
            return await Execute<T>(content, contentDescription, new Dictionary<string, string>(), contentType);
        }

        public async Task<T> Execute<T>(string content, string contentDescription, IDictionary<string, string> metadata, string contentType)
            where T : ExtendedLogReference
        {
            var combinedMetadata = metadata;
            combinedMetadata["ContentDescription"] = contentDescription;
            if (ImplicitElkContext.Current != null)
            {
                combinedMetadata = combinedMetadata
                    .Concat(ImplicitElkContext.Current.ToDictionary())
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            _logger.LogInformationWithMetadata($"Uploading {contentDescription} to extended logging", combinedMetadata);

            try
            {
                var reference = await _extendedLoggingService.Execute(new ExtendedLoggingRequest
                {
                    Module = _module,
                    Application = _resolver.Resolve(),
                    Content = Encoding.Default.GetBytes(content),
                    Metadata = SanitizeMetadata(combinedMetadata),
                    ContentType = contentType
                });

                var withReferenceMetadata = combinedMetadata
                    .Concat(CreateReferenceMetadata(reference))
                    .ToDictionary(x => x.Key, x => x.Value);

                _logger.LogInformationWithMetadata($"Uploaded {contentDescription} to extended logging", withReferenceMetadata);

                return reference as T;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed to upload {contentDescription} to extended logging", exception);
                return null;
            }
        }

        private static Dictionary<string, string> CreateReferenceMetadata(ExtendedLogReference reference)
        {
            return new Dictionary<string, string>
            {
                { "ExtendedLogReference.Name", reference.Name },
                { "ExtendedLogReference.Uri", reference.Uri?.AbsoluteUri }
            };
        }

        private static IDictionary<string, string> SanitizeMetadata(IDictionary<string, string> metadata)
        {

            return metadata.ToDictionary(
                kv => Uri.EscapeDataString(kv.Key.Replace(".", "_").Replace('-', '_')),
                kv => Uri.EscapeDataString(kv.Value ?? string.Empty));
        }
    }
}