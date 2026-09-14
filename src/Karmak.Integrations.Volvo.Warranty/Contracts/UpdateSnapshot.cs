using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public sealed class UpdateSnapshot : IEquatable<UpdateSnapshot>
    {
        public string Id { get; set; }
        public string OemId { get; set; }
        public string DealerCode { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset? ProcessedByFusionDateTime { get; set; }
        public UpdateType Type { get; set; }
        public string RepairOrderNumber { get; set; }
        public DateTime ProcessDate { get; set; }
        public Money ApprovedAmount { get; set; }
        public UpdateStatus Status { get; set; }
        public IEnumerable<UpdateException> Exceptions { get; set; }
        public IEnumerable<Deductible> Deductibles { get; set; }
        public IEnumerable<Tax> Taxes { get; set; }
        public Money LaborAmount { get; set; }
        public Money PartsAmount { get; set; }
        public Money OtherAmount { get; set; }
        public IEnumerable<Reconciliation.PartExpense> PartExpenses { get; set; }
        public IEnumerable<Reconciliation.LaborExpense> LaborExpenses { get; set; }
        public IEnumerable<Reconciliation.MiscellaneousExpense> MiscellaneousExpenses { get; set; }

        //exclude id, createdDateTime and processedByFusionDateTime from equality check
        public bool Equals(UpdateSnapshot other)
        {
            var oemId = EqualityExtensions.Equals(OemId, other?.OemId);
            var dealerCode = EqualityExtensions.Equals(DealerCode, other?.DealerCode);
            var type = Type.Equals(other?.Type);
            var repairOrderNumber = EqualityExtensions.Equals(RepairOrderNumber, other?.RepairOrderNumber);
            var approvedAmount = EqualityExtensions.Equals(ApprovedAmount, other?.ApprovedAmount);
            var status = EqualityExtensions.Equals(Status, other?.Status);
            var exceptions = EqualityExtensions.Equals(Exceptions, other?.Exceptions);
            var deductibles = EqualityExtensions.Equals(Deductibles, other?.Deductibles);
            var taxes = EqualityExtensions.Equals(Taxes, other?.Taxes);
            var laborAmount = EqualityExtensions.Equals(LaborAmount, other?.LaborAmount);
            var partsAmount = EqualityExtensions.Equals(PartsAmount, other?.PartsAmount);
            var otherAmount = EqualityExtensions.Equals(OtherAmount, other?.OtherAmount);
            var partExpenses = EqualityExtensions.Equals(PartExpenses, other?.PartExpenses);
            var laborExpenses = EqualityExtensions.Equals(LaborExpenses, other?.LaborExpenses);
            var miscExpenses = EqualityExtensions.Equals(MiscellaneousExpenses, other?.MiscellaneousExpenses);

            //can't use nullable datetime with DateTime.Compare
            //ternary allows us to avoid null ref exception
            var processDate = other == null ? DateTime.Compare(ProcessDate, default) == 0 : DateTime.Compare(ProcessDate, other.ProcessDate) == 0;

            var amounts = laborAmount && partsAmount && otherAmount && approvedAmount;
            var expenses = partExpenses && laborExpenses && miscExpenses;

            return oemId && dealerCode && type && repairOrderNumber && processDate && amounts && status && exceptions && deductibles && taxes && expenses;
        }

        public bool IsDuplicate(UpdateSnapshot y)
        {
            return Equals(y);
        }
    }
}
