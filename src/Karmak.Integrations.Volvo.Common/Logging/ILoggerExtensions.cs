using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Common.Logging
{
    public static class ILoggerExtensions
    {
        public static void LogInformationWithMetadata(this ILogger logger, string message, IDictionary<string, string> metadata)
        {
            //Needs to be <string, object> to be attached to the log event as properties
            var convertedMeta = metadata
                .Select(m => new KeyValuePair<string, object>(m.Key, m.Value))
                .ToList();

            using (var scope = logger.BeginScope(convertedMeta))
            { 
                logger.LogInformation(message);
            }
        }

        public static void LogErrorWithMetadata(this ILogger logger, string message, Exception exception, IDictionary<string, string> metadata)
        {
            //Needs to be <string, object> to be attached to the log event as properties
            var convertedMeta = metadata
                .Select(m => new KeyValuePair<string, object>(m.Key, m.Value))
                .ToList();

            using (var scope = logger.BeginScope(convertedMeta))
            {
                logger.LogError(exception, message);
            }
        }
    }
}
