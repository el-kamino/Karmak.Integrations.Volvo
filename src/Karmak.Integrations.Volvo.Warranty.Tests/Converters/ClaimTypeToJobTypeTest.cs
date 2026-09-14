using Karmak.Integrations.Volvo.Warranty.Converters;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class ClaimTypeToJobTypeTest
    {
        [Theory]
        [InlineData(false, "", null)]
        [InlineData(false, null, null)]
        [InlineData(true, "", null)]
        [InlineData(true, null, null)]
        public void WhenConvertingEmptyOrNullClaimType_It_ReturnsNull(bool inAppeal, string inputClaimType, string expectedClaimType)
        {
            var claim = FakeClaim.Generate();
            claim.InAppeal = inAppeal;
            claim.Type = inputClaimType;

            var result = new ClaimTypeToJobType().Convert(claim);

            Assert.Equal(expectedClaimType, result);
        }

        [Fact]
        public void WhenConvertingValidClaimType_It_ReturnsClaimType()
        {
            var claim = FakeClaim.Generate();
            claim.Type = "11";

            var result = new ClaimTypeToJobType().Convert(claim);

            Assert.Equal("11", result);
        }

        [Theory]
        [InlineData(true, "11", "A1")]
        [InlineData(false, "11", "11")]
        public void WhenConvertingAppealClaimType_It_ConvertsClaimTypeToA1(bool inAppeal, string inputClaimType, string expectedClaimType)
        {
            var claim = FakeClaim.Generate();
            claim.InAppeal = inAppeal;
            claim.Type = inputClaimType;

            var result = new ClaimTypeToJobType().Convert(claim);

            Assert.Equal(expectedClaimType, result);
        }
    }
}
