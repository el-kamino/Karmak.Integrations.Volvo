using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public sealed class Tax : IEquatable<Tax>
    {
        public IEnumerable<string> Description { get; set; }
        public Money Amount { get; set; }
        public decimal RatePercent { get; set; }

        //RatePercent is non-nullable
        public bool Equals(Tax other)
        {
            var amount = EqualityExtensions.Equals(Amount, other?.Amount);
            var rate = RatePercent.Equals(other?.RatePercent);
            var description = EqualityExtensions.Equals(Description, other?.Description);
            return amount && rate && description;
        }
    }
}
