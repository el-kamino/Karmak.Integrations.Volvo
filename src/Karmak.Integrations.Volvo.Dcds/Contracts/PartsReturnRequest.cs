namespace Karmak.Integrations.Volvo.Dcds.Contracts;

public class PartsReturnRequest 
{
    public DateTime RequestDate { get; set; }
    public decimal CreatedTimeZone { get; set; }
    public string? PACode { get; set; }
    public string? OrderNumber { get; set; }
    public string? VendorId { get; set; }
    public string? DistributionCode { get; set; }
    public List<ReturnPartItem>? PartsToReturn { get; set; }
    public string? KarmakAccountNumber { get; set; }
}