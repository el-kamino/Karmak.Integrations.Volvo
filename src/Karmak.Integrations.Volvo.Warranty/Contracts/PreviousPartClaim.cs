using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class PreviousPartClaim
    {
        public string InvoiceIdentifier { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Measurement Measurement { get; set; }
    }
}
