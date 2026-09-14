using System;
using System.Collections.Generic;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder
{
    public class FusionPartsSalesOrder {
        public string DatabaseVersion { get; set; }
        public string PoNumber { get; set; }
        public int SalesOrderNumber { get; set; }
        public DateTime? AddDateTime { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDateTime { get; set; }
        public decimal? InvoiceDateTimeZone { get; set; }
        public string OriginalInvoiceNumber { get; set; }
        public DateTime? OriginalInvoiceDateTime { get; set; }
        public decimal? OriginalInvoiceDateTimeZone { get; set; }
        public string SalesPerson { get; set; }
        public string Source { get; set; }
        public decimal? SalesTaxTotal { get; set; }
        public FusionCustomer BillingCustomer { get; set; }
        public FusionCustomer ShippingCustomer { get; set; }
        public IList<FusionPart> Parts { get; set; }
        public IList<FusionMiscCharge> MiscCharges { get; set; }
        public IList<FusionAddress> Addresses { get; set; }
        public string UpdateUserName { get; set; }
        public string OriginalSalesOrderNumber { get; set; }
    }
}
