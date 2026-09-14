using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Transportation
    {
        public string CarrierName { get; set; }
        public string InvoiceIdentifier { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public Damage Damage { get; set; }
    }
}
