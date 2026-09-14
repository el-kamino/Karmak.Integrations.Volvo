using System;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder
{
    public class FusionLabor {
        public decimal? UnitPrice { get; set; }
        public decimal? ExtendedPrice { get; set; }
        public decimal? TotalHours { get; set; }
        public string TechnicianUserName { get; set; }
        public int? TechnicianNumber { get; set; }
        public string TechnicianName { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string MiscPONumber { get; set; }
        public DateTime? MiscPODate { get; set; }
        public string TechnicianUserID { get; set; }
        public bool? IsBillingAdjustment { get; set; }
        public string DataState { get; set; }
    }
}