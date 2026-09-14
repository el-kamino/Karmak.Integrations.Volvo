using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.Common
{
    public class Vehicle {
        public string VIN { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string UnitInventoryIdentifier { get; set; }
        public string UnitNumber { get; set; }
        public VehicleCondition Condition { get; set; }
        public Odometer Odometer { get; set; }
        public Engine Engine { get; set; }
        public IList<ExternalIdentifier> ExternalIdentifiers { get; set; }
    }
}
