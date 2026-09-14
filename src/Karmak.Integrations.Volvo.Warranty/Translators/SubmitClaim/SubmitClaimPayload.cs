using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim
{
    public class SubmitClaimPayload
    {
        public SubmitClaimPayload() { Claims = new List<IClaim>(); }
        public SubmitClaimPayload(IClaim claim) { Claims = new List<IClaim>() { claim }; }
        public SubmitClaimPayload(IEnumerable<IClaim> claims) { Claims = claims; }

        public IEnumerable<IClaim> Claims { get; set; }
    }
}
