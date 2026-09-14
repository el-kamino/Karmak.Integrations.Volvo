using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using System.Security.Claims;

namespace Karmak.Integrations.Volvo.Api.Business
{
    public static class FusionIdentityExtensions
    {
        public static FusionIdentity GetFusionIdentity(this ClaimsPrincipal user)
        {
            ClaimsIdentity? fusionIdentity = user.Identities.FirstOrDefault(id => id.Label == "fusion");

            if (fusionIdentity == null)
            {
                throw new InvalidOperationException("no fusion identity");
            }

            return new FusionIdentity
            {
                AccountCode = GetClaim(fusionIdentity.Claims, "fusionKarmakAccountNumber"),
                BranchId = GetClaim(fusionIdentity.Claims, "fusionBranchId"),
                BranchCode = GetClaim(fusionIdentity.Claims, "fusionBranchCode"),
                UserId = GetClaim(fusionIdentity.Claims, "fusionUserId"),
                Username = GetClaim(fusionIdentity.Claims, "fusionUserName")
            };
        }

        private static string? GetClaim(IEnumerable<Claim> claims, string claimKey) =>
            claims.SingleOrDefault(claim => claim.Type == claimKey)?.Value;
    }
}
