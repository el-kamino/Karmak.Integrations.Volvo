using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.Validators;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Validators
{
    public class ReconciliationFetchRequestValidatorTest
    {

        private static readonly ReconciliationFetchRequestValidator _validator = new ReconciliationFetchRequestValidator();

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public static void WhenFetchRequestPACode_ISNullOrEmpty_ItFailsValidation(string paCode)
        {
            var fetch = new ReconciliationFetchRequest
            {
                StartDateTime = DateTime.Now.AddDays(-1),
                EndDateTime = DateTime.Now,
                PACode = paCode
            };

            var results = _validator.Validate(fetch);

            Assert.False(results.IsValid);
        }

        [Fact]
        public static void WhenStartDateTime_HasDefaultValue_ItFailsValidation()
        {
            var fetch = new ReconciliationFetchRequest
            {
                StartDateTime = default(DateTime),
                EndDateTime = DateTime.Now,
                PACode = "scoob"
            };

            var results = _validator.Validate(fetch);

            Assert.False(results.IsValid);
        }

        [Fact]
        public static void WhenEndDateTime_HasDefaultValue_ItFailsValidation()
        {
            var fetch = new ReconciliationFetchRequest
            {
                StartDateTime = DateTime.Now,
                EndDateTime = default(DateTime),
                PACode = "scoob"
            };

            var results = _validator.Validate(fetch);

            Assert.False(results.IsValid);
        }

        [Fact]
        public static void WhenEndDateTime_IsBeforeStartDateTime_ItFailsValidation()
        {
            var fetch = new ReconciliationFetchRequest
            {
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.AddDays(-1),
                PACode = "scoob"
            };

            var results = _validator.Validate(fetch);

            Assert.False(results.IsValid);
        }
    }
}
