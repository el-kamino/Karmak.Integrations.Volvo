namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;

public class FusionPartsInventoryReport
{
    public FusionPartInventoryMetaData PartInventoryMetaData { get; set; }
    public IList<FusionPart> Parts { get; set; }
}
