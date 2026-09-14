namespace Karmak.Integrations.Volvo.Api.Models.React;

public class CustomerUpdateRetransmitRequest
{
    public DateTime? WindowStart { get; set; }
    public DateTime? WindowEnd { get; set; }
    public string[] VolvoPassIds { get; set; }
    public string[] VINs { get; set; }
    public string PaCode { get; set; }

    public CustomerUpdateRetransmitRequest()
    {
        VolvoPassIds = Array.Empty<string>();
        VINs = Array.Empty<string>();
    }
}
