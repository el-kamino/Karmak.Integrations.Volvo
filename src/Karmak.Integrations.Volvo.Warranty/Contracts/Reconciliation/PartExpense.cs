using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation
{
    public sealed class PartExpense : IEquatable<PartExpense>
    {
        public string ItemId { get; set; }
        public string Description { get; set; }
        public Quantity Quantity { get; set; }
        public Quantity RequestedQuantity { get; set; }
        public Money Amount { get; set; }
        public Money RequestedAmount { get; set; }

        public bool Equals(PartExpense other)
        {
            var typeCodeValue = EqualityExtensions.Equals(ItemId, other?.ItemId);
            var description = EqualityExtensions.Equals(Description, other?.Description);
            var quantity = EqualityExtensions.Equals(Quantity, other?.Quantity);
            var amount = EqualityExtensions.Equals(Amount, other?.Amount);

            return typeCodeValue && description && quantity && amount;
        }
    }
}
