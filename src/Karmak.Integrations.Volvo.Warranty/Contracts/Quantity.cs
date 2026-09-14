using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public sealed class Quantity : IEquatable<Quantity>
    {
        public UnitOfMeasureType Type { get; set; }
        public decimal Value { get; set; }

        //Type and Value are non-nullable
        public bool Equals(Quantity other)
        {
            var type = Type.Equals(other.Type);
            var value = Value.Equals(other.Value);

            return type && value;
        }
    }
}

