using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public sealed class Deductible : IEquatable<Deductible>
    {
        public Money Amount { get; set; }
        public string Type { get; set; }

        public bool Equals(Deductible other)
        {
            var amount = EqualityExtensions.Equals(Amount, other?.Amount);
            var type = EqualityExtensions.Equals(Type, other?.Type);

            return amount && type;
        }
    }
}
