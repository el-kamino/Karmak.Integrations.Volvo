using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation
{
    public sealed class LaborExpense : IEquatable<LaborExpense>
    {
        public string OperationId { get; set; }
        public string Description { get; set; }
        public Quantity Hours { get; set; }
        public Quantity RequestedHours { get; set; }
        public Money Amount { get; set; }
        public Money RequestedAmount { get; set; }

        public bool Equals(LaborExpense other)
        {
            var operationId = EqualityExtensions.Equals(OperationId, other?.OperationId);
            var description = EqualityExtensions.Equals(Description, other?.Description);
            var hours = EqualityExtensions.Equals(Hours, other?.Hours);
            var amount = EqualityExtensions.Equals(Amount, other?.Amount);

            return operationId && description && hours && amount;
        }
    }
}
