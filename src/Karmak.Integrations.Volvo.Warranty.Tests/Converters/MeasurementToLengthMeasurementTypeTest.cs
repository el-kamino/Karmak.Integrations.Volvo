using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class MeasurementToLengthMeasurementTypeTest
    {
        [Fact]
        public void ConvertsMeasurementToLengthMeasureType()
        {
            var measurement = FakeMeasurement.GenerateMiles();

            var result = new MeasurementToMeasurementLengthType().Convert(measurement);

            Assert.Equal(LengthUnitsContentType.mile, result.unitCode);
            Assert.Equal(measurement.Value, result.Value);
        }

        [Fact]
        public void LengthMeasureTypeConversionHandlesNull()
        {
            var result = new MeasurementToMeasurementLengthType().Convert(null);

            Assert.Null(result);
        }
    }
}
