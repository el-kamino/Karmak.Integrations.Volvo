namespace Karmak.Integrations.Volvo.Dcds.Contracts;

public class ReturnPartItem 
{
    public string ReturnType { get; set; }
    public string PartNumber { get; set; }
    public int QuantityToReturn { get; set; }
    public string BinNumber { get; set; }
}