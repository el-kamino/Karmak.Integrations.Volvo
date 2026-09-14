namespace Karmak.Integrations.Volvo.Dcds.Contracts;

public class FinancialStatementRequest 
{
    public DateTime? RequestDate { get; set; }
    public string? PACode { get; set; }
    public string? FormattedFileContents { get; set; }
    public string? KarmakAccountNumber { get; set; }
}