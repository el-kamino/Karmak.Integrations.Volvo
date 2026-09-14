using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Mapping
{
    public sealed class ReconciliationToUpdateSnapshotMapperTest
    {
        private readonly ReconciliationToUpdateSnapshotMapper _mapper;
        private readonly RepairOrderReconciliationType _source;
        private readonly DateTime _processDate;
        private readonly string _paCode;
        private readonly Claim _claim;
        private readonly DateTime _createdDateTime;

        public ReconciliationToUpdateSnapshotMapperTest()
        {
            _mapper = new ReconciliationToUpdateSnapshotMapper();
            _source = FakeRepairOrderReconciliationType.Generate();
            _processDate = DateTime.UtcNow.Date;
            _paCode = "PA123";
            _claim = AutoFaker.Generate<Claim>();
            _createdDateTime = DateTime.UtcNow;
        }

        [Fact]
        public void WhenMapping_It_SetsTypeToReconciliation()
        {
            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            Assert.Equal(Karmak.Integrations.Volvo.Warranty.Contracts.UpdateType.Reconciliation, result.Type);
        }

        [Fact]
        public void WhenMapping_It_MapsRepairOrderNumber()
        {
            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            Assert.Equal(_source.DocumentID.Value, result.RepairOrderNumber);
        }

        [Fact]
        public void WhenMapping_It_MapsProcessDateAndDealerCode()
        {
            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            Assert.Equal(_processDate, result.ProcessDate);
            Assert.Equal(_paCode, result.DealerCode);
        }

        [Fact]
        public void WhenMapping_It_MapsAmounts()
        {
            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            var jobReconciliation = _source.JobReconciliation.First();
            Assert.Equal(jobReconciliation.ApprovedAmount.Value, result.ApprovedAmount.Value);
            Assert.Equal(jobReconciliation.LaborAmount.Value, result.LaborAmount.Value);
            Assert.Equal(jobReconciliation.PartsAmount.Value, result.PartsAmount.Value);
            Assert.Equal(jobReconciliation.OtherAmount.Value, result.OtherAmount.Value);
        }

        [Fact]
        public void WhenMapping_It_MapsExceptions()
        {
            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            var expectedExceptions = _source.JobReconciliation.First().DispositionReason.First().ExceptionCodes;
            var resultExceptions = result.Exceptions.ToList();

            Assert.Equal(expectedExceptions.Length, resultExceptions.Count);
            Assert.Equal(expectedExceptions[0].Code.Value, resultExceptions[0].Code);
            Assert.Equal(expectedExceptions[0].ExceptionText.Value, resultExceptions[0].Description);
        }

        [Theory]
        [InlineData("01", "Paid")]
        [InlineData("AE", "Approved - ESP Repairs")]
        [InlineData("A", "Approved - Other Repairs")]
        [InlineData("ZZ", "Other")]
        public void WhenMapping_It_MapsStatusFromCode(string statusCode, string expectedDescription)
        {
            _source.JobReconciliation.First().ClaimStatusCode = statusCode;

            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            Assert.Equal(statusCode, result.Status.Code);
            Assert.Equal(expectedDescription, result.Status.Description);
        }

        [Fact]
        public void WhenMapping_It_GeneratesId()
        {
            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            var repairOrderNumber = _source.DocumentID.Value;
            var jobNumber = _source.JobReconciliation.First().JobNumberString;
            var expected = string.Join("-", _createdDateTime.ToString(), _processDate.ToString(), _paCode, repairOrderNumber, jobNumber);

            Assert.Equal(expected, result.Id);
        }

        [Fact]
        public void WhenMapping_It_GeneratesOemId()
        {
            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            var repairOrderNumber = _source.DocumentID.Value;
            var jobNumber = _source.JobReconciliation.First().JobNumberString;
            var expected = string.Join("-", _processDate.ToString(), _paCode, repairOrderNumber, jobNumber);

            Assert.Equal(expected, result.OemId);
        }

        [Fact]
        public void WhenMappingWithNullJobReconciliation_It_HandlesGracefully()
        {
            _source.JobReconciliation = null;

            var result = _mapper.Map(_source, _processDate, _paCode, _claim, _createdDateTime);

            Assert.Equal(Karmak.Integrations.Volvo.Warranty.Contracts.UpdateType.Reconciliation, result.Type);
            Assert.Equal(_source.DocumentID.Value, result.RepairOrderNumber);
            Assert.Null(result.ApprovedAmount);
            Assert.Null(result.LaborAmount);
            Assert.Null(result.PartsAmount);
            Assert.Null(result.OtherAmount);
            Assert.Empty(result.Exceptions);
            Assert.Empty(result.Taxes);
            Assert.Empty(result.MiscellaneousExpenses);
            Assert.Empty(result.LaborExpenses);
            Assert.Empty(result.PartExpenses);
            Assert.Null(result.Status);
            Assert.Null(result.Id);
            Assert.Null(result.OemId);
        }
    }
}
