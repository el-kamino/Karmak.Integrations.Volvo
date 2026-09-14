using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data
{
    public class RepairOrderTask {
        public int? TaskNumber { get; set; }
        public string RepairTaskStatus { get; set; }
        public string RepairTaskSubStatus { get; set; }
        public IList<Part> Parts { get; set; }
        public IList<MiscCharge> MiscCharges { get; set; }
        public IList<Labor> LaborEntries { get; set; }
        public bool? Warranty { get; set; }
        public string RepairType { get; set; }
        public string SRTID { get; set; }
        public string RepairTypeDescription { get; set; }
        public int RepairTypeId { get; set; }
        public decimal TaskTaxTotal { get; set; }
        public decimal LaborRate { get; set; }
        public decimal OverrideLaborRate { get; set; }
        public string AlternateBillingCustomerKey { get; set; }
        public string CauseDescription { get; set; }
        public string ComplaintDescription { get; set; }
        public string CorrectionDescription { get; set; }
        public string ComplaintNotes { get; set; }
        public string TechnicianNotes { get; set; }
        public string InternalDealerNotes { get; set; }
        public IList<LaborOperation> LaborOperations { get; set; }
        public int? DepartmentID { get; set; }
        public string Department { get; set; }

        public RepairOrderTask() {
            Parts = new List<Part>();
            MiscCharges = new List<MiscCharge>();
            LaborEntries = new List<Labor>();
        }
    }
}