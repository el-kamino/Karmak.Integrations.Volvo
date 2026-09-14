namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared
{
    public class FusionVehicle {
        public string UnitID { get; set; }
        public string VIN { get; set; }
        public decimal? CurrentMeterReading { get; set; }
        public string CurrentMeterType { get; set; }
        public int? UnitInventoryID { get; set; }
        public string UnitNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public bool IsNewUnit { get; set; }
        public FusionEngine Engine { get; set; }
    }
}
