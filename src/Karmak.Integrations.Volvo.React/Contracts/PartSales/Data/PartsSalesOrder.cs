using System;
using System.Collections.Generic;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.React.Contracts.PartSales.Data
{
    public class PartsSalesOrder: IOverridable {
        public DealerInfo DealerInfo { get; set; }
        public PayloadMetadata Metadata { get; set; }
        public bool ForceTransmission { get; set; }
        public DateTime? AddDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerPurchaseOrderNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string OriginalInvoiceNumber { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }
        public decimal TaxTotal { get; set; }
        public Customer BillingCustomer { get; set; }
        public Customer ShipToCustomer { get; set; }
        public Contact Contact { get; set; }
        public string SalesPersonId { get; set; }
        public IList<Part> Parts { get; set; }
        public IList<MiscCharge> MiscCharges { get; set; }
        public string SaleTypeDescription { get; set; }
        public string Source { get; set; }
        public string PartPersonID { get; set; }
        public string SalesOrderStatus { get; set; }
        public string PartsOrderNumber { get; set; }
        public string OriginalPartsOrderNumber { get; set; }
        public decimal? TimeZone { get; set; }

        public PartsSalesOrder() {
            Parts = new List<Part>();
        }
    }
}