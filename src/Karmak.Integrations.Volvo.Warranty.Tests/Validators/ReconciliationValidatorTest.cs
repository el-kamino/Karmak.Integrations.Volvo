using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Validators;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Validators
{
    public class ReconciliationValidatorTest
    {
        [Fact]
        public void WhenMissingClaimId_It_IsNotValid()
        {
            var reconciliation = AutoFaker.Generate<UpdateSnapshotMessage>();
            reconciliation.ClaimId = null;

            var result = new ReconciliationValidator().Validate(reconciliation);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void WhenUpdateSnapshotIsNull_It_IsNotValid()
        {
            var reconciliation = AutoFaker.Generate<UpdateSnapshotMessage>();
            reconciliation.UpdateSnapshot = null;

            var result = new ReconciliationValidator().Validate(reconciliation);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void WhenMissingApprovedAmount_It_IsNotValid()
        {
            var reconciliation = AutoFaker.Generate<UpdateSnapshotMessage>();
            reconciliation.UpdateSnapshot.ApprovedAmount = null;

            var result = new ReconciliationValidator().Validate(reconciliation);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingTheRepairOrderNumber_It_IsNotValid(string value)
        {
            var reconciliation = AutoFaker.Generate<UpdateSnapshotMessage>();
            reconciliation.UpdateSnapshot.RepairOrderNumber = value;

            var result = new ReconciliationValidator().Validate(reconciliation);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingTheDealerCode_It_IsNotValid(string value)
        {
            var reconciliation = AutoFaker.Generate<UpdateSnapshotMessage>();
            reconciliation.UpdateSnapshot.DealerCode = value;

            var result = new ReconciliationValidator().Validate(reconciliation);

            Assert.False(result.IsValid);
        }
    }
}
