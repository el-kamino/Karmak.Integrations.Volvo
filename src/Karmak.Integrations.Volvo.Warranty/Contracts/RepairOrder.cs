using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Karmak.Integrations.Volvo.React.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{

    [KnownType(typeof(PartExpense))]
    [KnownType(typeof(LaborExpense))]
    [KnownType(typeof(MiscellaneousExpense))]
    public class RepairOrder
    {
        public string Identifier { get; set; }
        public IList<ExternalIdentifier> ExternalIdentifiers { get; set; }
        public string SecondaryIdentifier { get; set; }
        public IList<ExternalIdentifier> SecondaryExternalIdentifiers { get; set; }
        public User Advisor { get; set; }
        public IEnumerable<Job> Jobs { get; set; }
        public DateTime? OpenedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? InvoicedDate { get; set; }
        public string InvoiceIdentifier { get; set; }
        public IEnumerable<string> ApprovalIdentifiers { get; set; }
        public IList<Expense> Expenses
        {
            get =>
                (Jobs?.SelectMany(job => job.Expenses) ?? Enumerable.Empty<Expense>()).ToList();
        }

        public bool IsExtendedServiceContractFranchise { get; set; }
    }
}
