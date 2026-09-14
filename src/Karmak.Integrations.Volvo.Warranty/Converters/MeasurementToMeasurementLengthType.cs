using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class MeasurementToMeasurementLengthType : IConvertible<Measurement, LengthMeasureType>
    {
        public LengthMeasureType Convert(Measurement source) =>
            Conversions.GetOrNull(source,
                s => s != null && s != Measurement.Null,
                s => new LengthMeasureType
                {
                    unitCode = Conversions.UnitOfMeasureTypeToLengthUnitsContentType.Convert(s.UnitOfMeasure),
                    Value = s.Value
                });
    }
}
