using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Events
{
    public class ClaimSaved : IClaimSaved
    {
        public ClaimSaved(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public string Id { get; }
    }
}
