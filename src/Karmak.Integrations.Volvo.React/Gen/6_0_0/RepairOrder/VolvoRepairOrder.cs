using Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0;
using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Core.Gen.RepairOrders.V6_0_0
{
    public class VolvoRepairOrderTransmission
    {
        public VolvoHeader header { get; set; }
        public VolvoSender sender { get; set; }
        public List<VolvoPayload> payload { get; set; }
    }

    public class VolvoHeader
    {
        public string partyId { get; set; }
        public string siteCode { get; set; }
        public string resource { get; set; }
        public string requestId { get; set; }
        public string dealerCountryCode { get; set; }
        public string dealerNumberId { get; set; }
        public string storeNumber { get; set; }
        public decimal? roTotal { get; set; }
    }

    public class VolvoPayload
    {
        //Commented properties are not being sent by volvo as part of the payload, so not adding to the model for now.
        //If needed in future, can be added back.

        public string documentDateTime { get; set; }
        public string documentId { get; set; }
        public string customerPurchaseOrderNumber { get; set; }
        //public string invoiceNumberSystemId { get; set; }
        public string secondaryReferenceNumberString { get; set; }
        public string statusText { get; set; }
        public string roTypeCode { get; set; }
        public string roType { get; set; }
        //public string vehiclePickupDateTime { get; set; }
        //public string serviceLeadId { get; set; }
        //public string customerAppointmentNumber { get; set; }
        //public string customerAppointmentSystemId { get; set; }
        public string promisedRepairCompletionDateTime { get; set; }
        public string vehicleHatNumber { get; set; }
        public string repairOrderOpenedDateTime { get; set; }
        public string repairOrderCompletedDateTime { get; set; }
        //public string appointmentType { get; set; }
        public string appointmentScheduledDateTime { get; set; }
        //public string additionalWorkRequestedDateTime { get; set; }
        public string repairOrderInvoiceDateTime { get; set; }
        //public string dateAppointmentInitiated { get; set; }
        public List<VolvoCustomerParty> customerParty { get; set; }
        public VolvoVehicle vehicle { get; set; }
        public VolvoDistanceMeasure inDistanceMeasure { get; set; }
        //public VolvoDistanceMeasure outDistanceMeasure { get; set; }
        public List<VolvoPartyIdentifier> serviceAdvisorParty { get; set; }
        //public bool? waiterIndicator { get; set; }
        //public bool? surchargeInvoiceIndicator { get; set; }
        //public string orderNotes { get; set; }
        //public string orderInternalNotes { get; set; }
        public string departmentType { get; set; }
        public string dealerCustomerTypeCode { get; set; }
        public string dealerCustomerTypeName { get; set; }
        //public bool? vorIndicator { get; set; }
        public VolvoPrice price { get; set; }
        public decimal? taxAmount { get; set; }
        //public VolvoAddress locationAddress { get; set; }
        public List<VolvoJob> job { get; set; }
    }

    public class VolvoJob
    {
        //Commented properties are not being sent by volvo as part of the payload, so not adding to the model for now.
        //If needed in future, can be added back.

        public string jobNumberString { get; set; }
        public string operationId { get; set; }
        public VolvoCodesAndCommentsExpanded codesAndCommentsExpanded { get; set; }
        public List<VolvoServicePart> serviceParts { get; set; }
        public VolvoServiceLabor serviceLabor { get; set; }
        //public VolvoWarrantyClaim warrantyClaim { get; set; }
        public string jobStatusCode { get; set; }
        //public string bodyPaintType { get; set; }
        //public string bodyPaintCheck { get; set; }
        //public VolvoPartyIdentifier teamLeaderParty { get; set; }
        public string lineNumber { get; set; }
        //public string statementDescription { get; set; }
        public string technicianStartsJobSignInDateTime { get; set; }
        public string technicianFinishesJobSignOutDateTime { get; set; }
        public string dealerPaymentCode { get; set; }
        public string dealerPaymentName { get; set; }
    }

    public class VolvoPrice
    {
        public decimal? additionalWorkRequestEstimatedAmount { get; set; }
        public decimal? estimatedAmount { get; set; }
        public decimal? totalCustomerRepairOrderPrice { get; set; }
        public decimal? totalWarrantyRepairOrderPrice { get; set; }
        public decimal? totalInternalRepairOrderPrice { get; set; }
        public decimal? totalExtendedservicePlanRepairOrderPrice { get; set; }
        public decimal? totalAfterWarrantyAssistanceRepairOrderPrice { get; set; }
        public decimal? totalRepairOrderPrice { get; set; }
        public decimal? grossDiscountAmount { get; set; }
    }

    public class VolvoCodesAndCommentsExpanded
    {
        public string causeDescription { get; set; }
        //public string complaintCode { get; set; }
        public string complaintDescription { get; set; }
        public string correctionDescription { get; set; }
        //public string technicianNotes { get; set; }
        //public string miscellaneousNotes { get; set; }
        //public string conditionCode { get; set; }
    }

    public class VolvoServicePart
    {
        public string itemIdDescription { get; set; }
        public decimal? itemQuantity { get; set; }
        public VolvoServicePartPrice price { get; set; }
        //public string originalSerialNumberString { get; set; }
        //public string replacementSerialNumberString { get; set; }
        public List<VolvoPartsIdentifier> partsId { get; set; }
        //public string causeCode { get; set; }
        public string partsReturnDestinationCode { get; set; }
        public string partAddedToRODateTime { get; set; }
        public List<VolvoPartyIdentifier> soldByParty { get; set; }
        //public VolvoVehicleOffRoad vehicleOffRoad { get; set; }
    }

    //public class VolvoVehicleOffRoad
    //{
    //    public string vorReasonCode { get; set; }
    //    public string vorReasonDescription { get; set; }
    //    public string vorDepartmentCode { get; set; }
    //    public string vorDepartmentDescription { get; set; }
    //    public string vorPartsId { get; set; }
    //    public string partsOrderDate { get; set; }
    //    public string partsOrderType { get; set; }
    //    public string partsRegistrationNumber { get; set; }
    //    public string dealerOrderNumber { get; set; }
    //    public decimal? partsRequestedQuantity { get; set; }
    //    public decimal? partsIssuedQuantity { get; set; }
    //}

    public class VolvoServiceLabor
    {
        public string laborOperationId { get; set; }
        public string laborOperationTypeCode { get; set; }
        public string laborOperationDescription { get; set; }
        public decimal? chargeAmount { get; set; }
        public decimal? laborActualHoursNumeric { get; set; }
        //public decimal? laborAllowanceHoursNumeric { get; set; }
        public List<VolvoPartyIdentifier> serviceTechnicianParty { get; set; }
        public List<VolvoSublet> subletList { get; set; }
        public string workshopCode { get; set; }
        public string dealerWorkShopCode { get; set; }
        public string dealerWorkName { get; set; }
    }

    public class VolvoDistanceMeasure
    {
        public decimal? value { get; set; }
        public string unit { get; set; }
    }

    public class VolvoSublet
    {
        public string priceCode { get; set; }
        public decimal? chargeAmount { get; set; }
        public string subletWorkDescription { get; set; }
        //public VolvoServiceComponents serviceComponents { get; set; }
        //public VolvoRentLoaner rentLoaner { get; set; }
    }

    public class VolvoRentLoaner
    {
        public decimal? rentDaysQuantityNumeric { get; set; }
        public string rentInDate { get; set; }
        public string rentOutDate { get; set; }
    }

    public class VolvoWarrantyClaim
    {
        public string claimTypeString { get; set; }
        public string externalReferenceNumberString { get; set; }
        public string oemClaimNumberString { get; set; }
        public string claimCategoryString { get; set; }
        public decimal? batteryProrationNumeric { get; set; }
        public string priorClaimNumberString { get; set; }
    }

    public class VolvoServicePartPrice
    {
        public decimal? partCost { get; set; }
        public decimal? extendedAmount { get; set; }
    }

    public class VolvoServiceComponents
    {
        public decimal? expenseHoursNumeric { get; set; }
    }
}
