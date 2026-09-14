using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Validators;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Validators
{
    public class StatusValidatorTest
    {
        [Fact]
        public void WhenMissingClaimId_It_IsNotValid()
        {
            var status = AutoFaker.Generate<UpdateSnapshotMessage>();
            status.ClaimId = null;

            var result = new StatusValidator().Validate(status);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void WhenMissingStatus_It_IsNotValid()
        {
            var status = AutoFaker.Generate<UpdateSnapshotMessage>();
            status.UpdateSnapshot.Status = null;

            var result = new StatusValidator().Validate(status);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingTheRepairOrderNumber_It_IsNotValid(string value)
        {
            var status = AutoFaker.Generate<UpdateSnapshotMessage>();
            status.UpdateSnapshot.RepairOrderNumber = value;

            var result = new StatusValidator().Validate(status);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingStatusCode_It_IsNotValid(string value)
        {
            var status = AutoFaker.Generate<UpdateSnapshotMessage>();
            status.UpdateSnapshot.Status.Code = value;

            var result = new StatusValidator().Validate(status);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingStatusDescription_It_IsNotValid(string value)
        {
            var status = AutoFaker.Generate<UpdateSnapshotMessage>();
            status.UpdateSnapshot.Status.Description = value;

            var result = new StatusValidator().Validate(status);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingTheDealerCode_It_IsNotValid(string value)
        {
            var status = AutoFaker.Generate<UpdateSnapshotMessage>();
            status.UpdateSnapshot.DealerCode = value;

            var result = new StatusValidator().Validate(status);

            Assert.False(result.IsValid);
        }
    }
}
