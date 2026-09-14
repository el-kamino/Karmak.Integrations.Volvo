using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder
{
    public class FusionRepairOrderTask {
        public int? TaskNumber { get; set; }
        public string RepairTaskStatus { get; set; }
        public string RepairTaskSubStatus { get; set; }
        public IList<FusionPart> Parts { get; set; }
        public IList<FusionMiscCharge> MiscCharges { get; set; }
        public IList<FusionLabor> LaborEntries { get; set; }
        public bool? Warranty { get; set; }
        public string RepairType { get; set; }
        public string SRTID { get; set; }
        public string RepairTypeDescription { get; set; }
        public int RepairTypeId { get; set; }
        public decimal? TaskTaxTotal { get; set; }
        public decimal? LaborRate { get; set; }
        public decimal? OverrideLaborRate { get; set; }
        public string AlternateBillingCustomerKey { get; set; }
        public FusionComplaintCauseCorrection ComplaintCauseCorrection { get; set; }
        public IList<FusionSRT> SRTs { get; set; }
        public int? DepartmentID { get; set; }
        public string Department { get; set; }
    }
}