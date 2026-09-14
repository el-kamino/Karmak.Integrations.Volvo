using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Validators;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Validators
{
    public class WarrantyPaymentInformationValidatorTest
    {

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingTheRepairOrderNumber_It_IsNotValid(string value)
        {
            var payment = AutoFaker.Generate<WarrantyPaymentInformation>();
            payment.RepairOrderNumber = value;

            var result = new WarrantyPaymentInformationValidator().Validate(payment);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingClaimNumber_It_IsNotValid(string value)
        {
            var payment = AutoFaker.Generate<WarrantyPaymentInformation>();
            payment.ClaimNumber = value;

            var result = new WarrantyPaymentInformationValidator().Validate(payment);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingDealerCode_It_IsNotValid(string value)
        {
            var payment = AutoFaker.Generate<WarrantyPaymentInformation>();
            payment.DealerCode = value;

            var result = new WarrantyPaymentInformationValidator().Validate(payment);

            Assert.False(result.IsValid);
        }

    }
}
