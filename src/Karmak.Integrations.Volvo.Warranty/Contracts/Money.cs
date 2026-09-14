using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public sealed class Money : IMoney, IEquatable<Money>
    {
        public CurrencyCode Currency { get; set; }
        public decimal Value { get; set; }

        //Currency and Value are non-nullable
        public bool Equals(Money other)
        {
            var currency = Currency.Equals(other?.Currency);
            var value = Value.Equals(other?.Value);

            return currency && value;
        }
    }
}
