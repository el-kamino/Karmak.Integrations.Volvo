using Karmak.Integrations.Volvo.Dcds.Api;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.FinancialStatement;

public class SubmitVolvoFinancialStatementRequest : ISendFileRequest
{
    public string SendType { get; set; }
    public string SendTypeVersion { get; set; }
    public string FileName { get; set; }
    public string DealerId { get; set; }
    public string Detail { get; set; }
    public string AccountNumber { get; set; }
    public string KarmakAccountNumber { get; set; }
}