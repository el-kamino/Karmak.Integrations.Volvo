using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Contracts
{
    public class UpdateStatusTest
    {

        [Fact]
        public void StatusDescriptionShouldBeExcluded_From_EqualityCheck()
        {
            var statusWithDescription = new UpdateStatus
            {
                Code = "A",
                Description = "I'm a description"
            };

            var statusWithoutDescription = new UpdateStatus
            {
                Code = "A"
            };

            Assert.True(statusWithDescription.Equals(statusWithoutDescription));
        }

        [Fact]
        public void StatusesWithDifferentCodes_AreNot_Equivalent()
        {
            var statusWithCode = new UpdateStatus
            {
                Code = "A",
            };

            var statusWithDifferentCode = new UpdateStatus
            {
                Code = "B"
            };

            Assert.False(statusWithCode.Equals(statusWithDifferentCode));
        }
    }
}
