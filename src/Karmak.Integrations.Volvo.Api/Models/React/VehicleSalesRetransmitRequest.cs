namespace Karmak.Integrations.Volvo.Api.Models.React
{
    public class VehicleSalesRetransmitRequest
    {
        public DateTime? WindowStart { get; set; }
        public DateTime? WindowEnd { get; set; }
        public string[] InvoiceNumbers { get; set; }
        public string PaCode { get; set; }

        public VehicleSalesRetransmitRequest()
        {
            InvoiceNumbers = Array.Empty<string>();
        }
    }
}