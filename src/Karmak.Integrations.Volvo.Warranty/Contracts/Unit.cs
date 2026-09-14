using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Unit
    {
        public Unit()
        {
            MeterReadings = new List<MeterReading>();
        }

        public string Identifier { get; set; }
        public IList<ExternalIdentifier> ExternalIdentifiers { get; set; }
        public Vehicle Vehicle { get; set; }
        public IEnumerable<MeterReading> MeterReadings { get; set; }

        public MeterReading GetReading(MeterReadingType type) =>
            MeterReadings.FirstOrDefault(reading => reading.Type == type)
            ?? MeterReading.CreateWithNullReadings(type);
    }
}
