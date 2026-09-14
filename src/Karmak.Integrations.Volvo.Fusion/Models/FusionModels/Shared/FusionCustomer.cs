using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared
{
    public class FusionCustomer {
        public string CustomerID { get; set; }
        public string CustomerKey { get; set; }
        public string CompanyName { get; set; }
        public IList<FusionContact> Contacts { get; set; }
        public IList<FusionAddress> Addresses { get; set; }
        public string CustomerTypeCode { get; set; }
        public string IndustryType { get; set; }
        public string BusinessStructure { get; set; }
        public string OfficePhone { get; set; }
        public string CellPhone { get; set; }
        public string EmailAddress { get; set; }
        public string SalesInvoiceEmailAddress { get; set; }
        public string PartsInvoiceEmailAddress { get; set; }
        public bool? IsInternalSalesAccount { get; set; }
        public bool? IsInternalLRAccount { get; set; }
        public IList<FusionExternalIdentifier> ExternalIdentifiers { get; set; }
    }
}