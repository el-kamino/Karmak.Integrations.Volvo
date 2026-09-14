using Microsoft.Azure.Relay;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace Karmak.Integrations.Volvo.Common.Bridge
{
    internal class JsonSecurityToken : SecurityToken
    {
        //This is almost a direct copy of Microsoft.Azure.Relay.JsonSecurityToken.  Hopefully at some point this package
        //will drop the custom SecurityToken and TokenProvider interfaces and use the standard interfaces in Azure.Identity.
        private readonly JwtSecurityToken internalToken;

        private readonly string audience;

        private readonly string rawToken;

        public override string Audience => audience;

        public override DateTime ExpiresAtUtc => internalToken.ValidTo;

        public override string TokenString => rawToken;

        internal JsonSecurityToken(string tokenString, string audience)
        {
            if (string.IsNullOrEmpty(tokenString))
            {
                throw new ArgumentNullException(nameof(tokenString));
            }

            internalToken = new JwtSecurityToken(tokenString);
            this.audience = audience;
            rawToken = tokenString;
        }
    }
}
