using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class UnitOfMeasureTypeToLengthUnitsContextTypeTest
    {
        [Theory]
        [InlineData(UnitOfMeasureType.Miles, LengthUnitsContentType.mile)]
        [InlineData(UnitOfMeasureType.Kilometers, LengthUnitsContentType.kilometer)]
        public void TranslatesKnownUnitOfMeasureTypesToStarRepresentations(UnitOfMeasureType unitOfMeasureType, LengthUnitsContentType starType)
        {
            var result = new UnitOfMeasureTypeToLengthUnitsContentType().Convert(unitOfMeasureType);

            Assert.Equal(starType, result);
        }

        [Fact]
        public void ThrowsArgumentExceptionIfUnitOfMeasureCannotBeTranslated()
        {
            var ex = Assert.Throws<ConversionException<UnitOfMeasureType, LengthUnitsContentType>>(() => new UnitOfMeasureTypeToLengthUnitsContentType().Convert(UnitOfMeasureType.None));

            Assert.Equal("Cannot cast value 'None' of type 'Karmak.Integrations.Volvo.Warranty.Contracts.UnitOfMeasureType' to 'Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5.LengthUnitsContentType'", ex.Message);
        }
    }
}
