using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation
{
    public sealed class MiscellaneousExpense : IEquatable<MiscellaneousExpense>
    {
        public string TypeCodeValue { get; set; }
        public string Description { get; set; }
        public Money Amount { get; set; }
        public Money RequestedAmount { get; set; }

        public bool Equals(MiscellaneousExpense other)
        {
            var typeCodeValue = EqualityExtensions.Equals(TypeCodeValue, other?.TypeCodeValue);
            var description = EqualityExtensions.Equals(Description, other?.Description);
            var amount = EqualityExtensions.Equals(Amount, other?.Amount);

            return typeCodeValue && description && amount;
        }
    }
}
