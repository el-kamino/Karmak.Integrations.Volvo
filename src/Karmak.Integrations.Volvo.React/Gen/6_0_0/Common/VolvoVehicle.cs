using System;

namespace Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0
{
    public class VolvoVehicle
    {
        public string model { get; set; }
        public string modelDescription { get; set; }
        public int modelYear { get; set; }
        public string makeString { get; set; }
        public string powerTrainType { get; set; }
        public string vehicleId { get; set; }
        public VolvoCertificationGroup certificationGroup { get; set; }
        public VolvoDeliveryDistanceMeasure deliveryDistanceMeasure { get; set; }
        public string fleetVehicleId { get; set; }
        public string licenseNumberString { get; set; }
        public VolvoEstimatedUsageDistanceMeasure estimatedUsageDistanceMeasure { get; set; }
    }

    public class VolvoCertificationGroup
    {
        public DateTime? certificationDate { get; set; }
        public string type { get; set; }
    }

    public class VolvoDeliveryDistanceMeasure
    {
        public decimal? value { get; set; }
        public string unit { get; set; }
    }

    public class VolvoEstimatedUsageDistanceMeasure
    {
        public decimal? value { get; set; }
        public string unit { get; set; }
    }
}
