using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsReturn;

public class PartPurchaseOrder
{
    public DateTime CreatedDateTime { get; set; }
    public decimal CreatedTimeZone { get; set; }
    public string PONumber { get; set; }
    public bool ReturnPurchaseOrder { get; set; }
    public List<PartPurchaseOrderDetail> Parts { get; set; }
    public List<Message> Messages { get; set; }
}
