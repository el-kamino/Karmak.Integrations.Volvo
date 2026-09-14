using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public interface IClaim
    {
        string Id { get; }
        string CorrelationId { get; }
        Company Oem { get; }
        string Identifier { get; }
        ClaimStatus Status { get; }
        string Type { get; }
        string SubCode { get; }
        Money Total { get; }
        SplitContribution Split { get; set; }
        Dealer Dealer { get; }
        RepairOrder RepairOrder { get; }
        Customer Customer { get; }
        Driver Driver { get; }
        Unit Unit { get; }
        FleetAccount FleetAccount { get; }
        bool IsManualReviewRequired { get; }
        bool IsRelatedDamageIncluded { get; }
        string PreAuthorizationIdentifier { get; }
        bool? ShouldHoldAtPreValidation { get; }
        string CreatedBy { get; }
        DateTimeOffset CreatedDateTime { get; }
        string UpdatedBy { get; }
        DateTimeOffset UpdatedDateTime { get; }
        string CausalPartIdentifier { get; }
        bool InAppeal { get; }
    }
}
