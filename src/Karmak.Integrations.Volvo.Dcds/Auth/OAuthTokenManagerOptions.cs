namespace Karmak.Integrations.Volvo.Dcds.Auth
{
    public class OAuthTokenManagerOptions
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string TokenUri { get; set; } 
        public required string InboundScope { get; set; }
        public required string OutboundScope { get; set; }
    }
}
