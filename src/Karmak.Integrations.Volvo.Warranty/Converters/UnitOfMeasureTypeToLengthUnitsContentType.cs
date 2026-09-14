using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class UnitOfMeasureTypeToLengthUnitsContentType : IConvertible<UnitOfMeasureType, LengthUnitsContentType>
    {
        private static readonly IDictionary<UnitOfMeasureType, LengthUnitsContentType> UnitMappings =
            new Dictionary<UnitOfMeasureType, LengthUnitsContentType>
            {
                [UnitOfMeasureType.Kilometers] = LengthUnitsContentType.kilometer,
                [UnitOfMeasureType.Miles] = LengthUnitsContentType.mile,
            };

        public LengthUnitsContentType Convert(UnitOfMeasureType source) =>
            UnitMappings.TryGetValue(source, out LengthUnitsContentType result)
                ? result
                : throw new ConversionException<UnitOfMeasureType, LengthUnitsContentType>(source);
    }
}
