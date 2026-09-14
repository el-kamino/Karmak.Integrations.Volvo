using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class AmountTypetoMoneyTest
    {

        [Fact]
        public void WhenConvertingNullSource_It_ReturnsNull()
        {
            var result = new AmountTypeToMoney().Convert(null);

            Assert.Null(result);
        }

        [Fact]
        public void WhenConvertingValidAmountType_It_ReturnsMoney()
        {
            var fakeAmount = new AmountType
            {
                Value = 2,
                currencyID = "USD"
            };

            var result = new AmountTypeToMoney().Convert(fakeAmount);

            Assert.Equal(fakeAmount.Value, result.Value);
            Assert.Equal(fakeAmount.currencyID, result.Currency.ToString());
        }
    }
}
