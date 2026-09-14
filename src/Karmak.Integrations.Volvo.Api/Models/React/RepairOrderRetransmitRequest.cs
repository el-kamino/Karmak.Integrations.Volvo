namespace Karmak.Integrations.Volvo.Api.Models.React;

public class RepairOrderRetransmitRequest
{
    public DateTime? WindowStart { get; set; }
    public DateTime? WindowEnd { get; set; }
    public string[] RepairOrderNumbers { get; set; }
    public string PaCode { get; set; }

    public RepairOrderRetransmitRequest()
    {
        RepairOrderNumbers = Array.Empty<string>();
    }
}