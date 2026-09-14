using Azure.Core;
using Microsoft.Azure.Relay;

namespace Karmak.Integrations.Volvo.Common.Bridge
{
    public class DefaultAzureCredentialTokenProvider : TokenProvider
    {
        //this class is a modified version of Microsoft.Azure.Relay.ManagedIdentityTokenProvider
        private static readonly TokenRequestContext TokenRequestContext = new TokenRequestContext(new string[1] { "https://relay.azure.net//.default" });

        private readonly TokenCredential _credential;

        public DefaultAzureCredentialTokenProvider(TokenCredential credential) 
        {
            _credential = credential;
        }

        protected override async Task<SecurityToken> OnGetTokenAsync(string audience, TimeSpan validFor)
        {
            return new JsonSecurityToken((await _credential.GetTokenAsync(TokenRequestContext, default)).Token, audience);
        }
    }
}
