namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// The claim columns a search may be expressed against. Search input names a member of this
/// enum rather than a column, so no part of a request ever reaches the generated sql as text.
/// </summary>
/// <remarks>
/// InstanceIdentifier, BranchIdentifier and IsDeleted are deliberately absent. The first two are
/// stamped from the caller's elk context and the third is controlled by
/// <see cref="WarrantyClaimSearchQuery.IncludeDeleted"/>, so none of them is the caller's to filter.
/// </remarks>
public enum WarrantyClaimField
{
    ClaimId,
    CorrelationId,
    BranchCode,
    CausalPartIdentifier,
    ClaimIdentifier,
    CompanyName,
    CustomerIdentifier,
    InvoiceIdentifier,
    Oem,
    RepairOrderIdentifier,
    WarrantyRepairOrderIdentifier,
    ClaimStatus,
    VehicleIdentifier,
    ClaimTotal,
    RepairOrderOpenedDate,
    RepairOrderCompletedDate,
    CreatedOn,
    UpdatedOn
}
