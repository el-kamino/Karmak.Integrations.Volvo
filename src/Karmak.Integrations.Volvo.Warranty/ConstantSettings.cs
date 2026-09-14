using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty
{
    public static class ConstantSettings
    {
        public const string VolvoCompanyName = "Volvo";
        public const string VolvoOneWarrantySystemVersion = "5.2.4";
        public const string VolvoProcessRepairOrderService = "ProcessRepairOrder";
        public const string VolvoGetServiceProcessingAdvisory = "GetServiceProcessingAdvisory";
        public const string TransmitWarrantyClaimNamespace = "urn:volvo/star/services/v1/ClaimRepairOrder";
        public const string ClaimReconciliationRequest = "urn:volvo/star/services/v1/ClaimReconciliation";
        public const string StarPutMessageAction = "http://www.starstandards.org/webservices/2005/10/transport/operations/PutMessage";
        public const string StarProcessMessageAction = "http://www.starstandards.org/webservices/2005/10/transport/operations/ProcessMessage";
        public const string DestinationNameCode = "FM";
        public const string DestinationSoftwareCode = "OWS";
        public const string DestinationSoftwareVersion = "1.0";
        public const string DestinationServiceMessageId = "ONE Warranty Solution";
        public const string VolvoClaimReconciliationDestinationServiceMessageId = "OWS Claim RECONCILIATION";
        public const string VolvoClaimReconciliationSenderServiceMessageId = "OWS Claim Reconciliation";
        public const string Default = "Default";
        public const string Never = "Never";
        public const string VolvoWarrantyProcessingTaskIdentifier = "WarrantyProcessing";
        public const string RepairOrderTaskIdentifier = "RepairOrder";
        public const string ProcessDate = "ProcessDate";
        public const string Claim = "Claim";
        public const string PaCode = "PaCode";
        public const string CreatedDateTime = "CreatedDateTime";
        public const ConfirmationEnumeratedType ConfirmationCode = ConfirmationEnumeratedType.Never;
        public const SplitsTypeEnumeratedType RepairOrderInvoiceSplitType = SplitsTypeEnumeratedType.Job;

        public const string CorePartPricingDescription = "CORE AMOUNT";
        public const string ExtendedPartPricingDescription = "PART EXTENDED AMOUNT";
        public const string LaborPricingDescription = "Labor Total";
        public const string VolvoSettingsKey = "VolvoSettings";
        public const CurrencyCode CurrencyCodeDefault = CurrencyCode.USD;
        public const string CustomerRegionKey = "CustomerRegion";
        public const string OwningCustomerAddressEntityType = "Repair Order Owning";
    }
}
