using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Claim : IClaim
    {
        public string CorrelationId { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        public Company Oem { get; set; }
        public string Identifier { get; set; }
        public ClaimStatus Status { get; set; }
        public bool InAppeal { get; set; }
        public string Type { get; set; }
        public string SubCode { get; set; }
        public Money Total =>
            new Money
            {
                Currency = CurrencyCode.USD,
                Value = RepairOrder?.Expenses?.Sum(expense => expense.Total?.Value) ?? 0m
            };
        public SplitContribution Split { get; set; }
        public Dealer Dealer { get; set; }
        public RepairOrder RepairOrder { get; set; }
        public Customer Customer { get; set; }
        public Driver Driver { get; set; }
        public Unit Unit { get; set; }
        public FleetAccount FleetAccount { get; set; }
        public bool IsManualReviewRequired { get; set; }
        public bool IsRelatedDamageIncluded { get; set; }
        public string PreAuthorizationIdentifier { get; set; }
        //should be at job level
        public bool? ShouldHoldAtPreValidation { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDateTime { get; set; }
        public string CausalPartIdentifier =>
            RepairOrder?
                .Jobs?
                .Select(job =>
                    job.PartExpenses?.Where(part => part.IsCausalPart).Select(part => part.Identifier).FirstOrDefault())
                .FirstOrDefault();
        public bool IsDeleted { get; set; }
        public IList<UpdateSnapshot> UpdateSnapshots { get; set; } = new List<UpdateSnapshot>();
    }
}
