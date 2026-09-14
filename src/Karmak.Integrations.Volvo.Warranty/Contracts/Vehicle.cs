using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Vehicle
    {
        public string Identifier { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public DateTime? InServiceDate { get; set; }
        public License License { get; set; }
        public string SpecialUseIdentifier { get; set; }
    }
}
