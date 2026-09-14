using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Common.Bridge
{
    public sealed class BridgeClient : IBridgeClient
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly ILogger<BridgeClient> _logger;
        private readonly IRelayBuilder _relayBuilder;

        public BridgeClient(
            ISettingsProvider settingsProvider,
            IRelayBuilder relayBuilder,
            ILogger<BridgeClient> logger)
        {
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _relayBuilder = relayBuilder;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TResponse> SubmitMessageAsync<TMessage, TResponse>(string contractName, TMessage message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new ArgumentNullException(nameof(contractName));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return SubmitMessageInternalAsync<TMessage, TResponse>(contractName, message, cancellationToken);
        }

        private async Task<TResponse> SubmitMessageInternalAsync<TMessage, TResponse>(string contractName, TMessage message, CancellationToken cancellationToken)
        {
            var meta = new Dictionary<string, string>
            {
                ["MessageType"] = typeof(TMessage).FullName,
                ["Fusion Contract"] = contractName,
                ["Internal CorrelationID"] = Guid.NewGuid().ToString()
            };

            _logger.LogInformationWithMetadata("Preparing to send message to Relay", meta);

            KicqSettings settings = await _settingsProvider.GetBridgeSettingsAsync();
            IRelaySender sender = _relayBuilder.Build(settings.BridgeSettings.ChannelName);

            var envelope = new RequestDetails
            {
                Content = JsonConvert.SerializeObject(message),
                Url = contractName,
                Headers = GetHeaders(settings, ImplicitElkContext.Current)
            };

            ResponseDetails response = await sender.SendRequest(envelope);
            var data = JsonConvert.DeserializeObject<TResponse>(envelope.Content);
            _logger.LogInformationWithMetadata("Finised sending message to Relay", meta);
            return data;
        }

        private static List<Header> GetHeaders(KicqSettings settings, ElkContext context)
        {
            return new List<Header>
            {
                new Header { Key = "account", Value = context.Identity.Account.ToString() },
                new Header { Key = "user", Value = context.Identity.User.ToString() },
                new Header { Key = "security_profile_account", Value = context.SecurityProfile.Account.ToString() },
                new Header { Key = "security_profile_user", Value = context.SecurityProfile.User.ToString() },
                new Header { Key = "fusionUser", Value = settings.FusionIdentity.Username },
                new Header { Key = "fusionUserId", Value = Convert.ToString(settings.FusionIdentity.UserId) },
                new Header { Key = "fusionUserLoginId", Value = Convert.ToString(settings.FusionIdentity.UserLoginId) },
                new Header { Key = "fusionConnectionId", Value = Convert.ToString(settings.FusionIdentity.ConnectionId) }
            };
        }
    }
}
