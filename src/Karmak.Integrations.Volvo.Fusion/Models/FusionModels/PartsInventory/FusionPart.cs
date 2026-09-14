namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;

public class FusionPart
{
    public decimal ReceivedToday { get; set; }
    public decimal SoldToday { get; set; }
    public decimal AdjustmentToday { get; set; }
    public string AdjustmentType { get; set; }
    public IList<FusionSalesHistory> SalesHistory { get; set; }
    public string PartNumber { get; set; }
    public string Description { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime? LastSold { get; set; }
    public string StockStatus { get; set; }
    public decimal StockingLevel { get; set; }
    public string PartType { get; set; }
    public string CorePartNumber { get; set; }
    public string BinLocation { get; set; }
    public bool IsVolvoRIMManaged { get; set; }
    public bool IsVolvoPart { get; set; }
    public string TriggerReasonCode { get; set; }
}
