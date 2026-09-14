using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.MessagingError;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Fusion.Common
{
    internal class MessagingErrorDispatcher : IRequestDispatcher
    {
        public const string EntityType = "Messaging Error";

        private readonly ILogger _logger;

        public MessagingErrorDispatcher(ILogger<MessagingErrorDispatcher> logger)
        {
            _logger = logger;
        }

        public Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
        {
            var payload = request.Payload.ToObject<PoisonMessage>();

            var metadata = new Dictionary<string, object> 
            {
                {"Metric","FusionTransmissionFailed" },
                {"Entity Type", request.EntityType},
                {"Entity ID", request.EntityId.ToString()},
                {"Branch ID", identity.BranchId},
                {"Branch Code", identity.BranchCode},
                {"Account Code", identity.AccountCode},
                {"User ID", identity.UserId},
                {"Username", identity.Username}
            };

            using (var scope = _logger.BeginScope(metadata))
            {
                foreach (Message m in payload.Messages)
                {
                    _logger.LogError(new Exception($"{m.MessageType}: {m.MessageText}"), "Fusion Error");
                }
            }

            return Task.CompletedTask;
        }
    }
}
