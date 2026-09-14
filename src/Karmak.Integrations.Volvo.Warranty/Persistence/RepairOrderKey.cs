namespace Karmak.Integrations.Volvo.Warranty.Persistence
{
    /// <summary>
    /// The branch and repair order a claim hangs off of.
    /// </summary>
    /// <param name="BranchIdentifier">Branch the claim was raised at.</param>
    /// <param name="RepairOrderIdentifier">The repair order's own identifier.</param>
    /// <param name="WarrantyRepairOrderIdentifier">
    /// The repair order's secondary identifier, which warranty treats as the repair order number.
    /// </param>
    public sealed record RepairOrderKey(
        string BranchIdentifier,
        string RepairOrderIdentifier,
        string WarrantyRepairOrderIdentifier);
}
