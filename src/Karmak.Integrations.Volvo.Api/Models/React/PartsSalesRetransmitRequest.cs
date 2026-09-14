namespace Karmak.Integrations.Volvo.Api.Models.React;

public class PartsSalesRetransmitRequest
{
    public DateTime? WindowStart { get; set; }
    public DateTime? WindowEnd { get; set; }
    public string[] InvoiceNumbers { get; set; }
    public string PaCode { get; set; }

    public PartsSalesRetransmitRequest()
    {
        InvoiceNumbers = Array.Empty<string>();
    }
}