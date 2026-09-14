using Karmak.Integrations.Volvo.Dcds.Api;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.PartsReturn;

public class ProcessPartsReturnRequest : ISendFileRequest
{
    public string? SenderID { get; set; }
    public string? DestinationID { get; set; }
    public string? DealerId { get; set; }
    public string? SendType { get; set; }
    public string? SendTypeVersion { get; set; }
    public string? FileName { get; set; }
    public string? Detail { get; set; }
    public string? OrderNumber { get; set; }
    public string? PACode { get; set; }
    public string? KarmakAccountNumber { get; set; }
}