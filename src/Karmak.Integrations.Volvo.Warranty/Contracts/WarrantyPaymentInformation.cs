using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class WarrantyPaymentInformation
    {
        public string Id { get; set; }
        public string DealerCode { get; set; }
        public string ClaimNumber { get; set; }
        public string RepairOrderNumber { get; set; }
        public DateTime ProcessDate { get; set; }
        public decimal ToBePaidAmount { get; set; }
        public int RepairOrderID { get; set; }
    }
}
