using System;

namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data
{
    public class Labor {
        public decimal? UnitPrice { get; set; }
        public decimal? ExtendedPrice { get; set; }
        public decimal? TotalHours { get; set; }
        public int? TechnicianNumber { get; set; }
        public string TechnicianUsername { get; set; }
        public string TechnicianFullName { get; set; }
        public DateTime? DateTimeIn { get; set; }
        public DateTime? DateTimeOut { get; set; }
        public string InvoiceIdentifier { get; set; }
        public DateTime? InvoiceDate { get; set; }
    }
}