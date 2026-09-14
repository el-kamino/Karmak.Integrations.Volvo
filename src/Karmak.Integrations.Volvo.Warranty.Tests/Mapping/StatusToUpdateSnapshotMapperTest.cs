using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Mapping
{
    public sealed class StatusToUpdateSnapshotMapperTest
    {
        private readonly StatusToUpdateSnapshotMapper _mapper;
        private readonly RepairOrderReconciliationType _source;
        private readonly DateTime _processDate;
        private readonly string _paCode;

        public StatusToUpdateSnapshotMapperTest()
        {
            _mapper = new StatusToUpdateSnapshotMapper();
            _source = FakeRepairOrderReconciliationType.Generate();
            _processDate = DateTime.UtcNow.Date;
            _paCode = "PA456";
        }

        [Fact]
        public void WhenMapping_It_SetsTypeToStatus()
        {
            var result = _mapper.Map(_source, _processDate, _paCode);

            Assert.Equal(Karmak.Integrations.Volvo.Warranty.Contracts.UpdateType.Status, result.Type);
        }

        [Fact]
        public void WhenMapping_It_MapsRepairOrderNumber()
        {
            var result = _mapper.Map(_source, _processDate, _paCode);

            Assert.Equal(_source.DocumentID.Value, result.RepairOrderNumber);
        }

        [Fact]
        public void WhenMapping_It_MapsProcessDateAndDealerCode()
        {
            var result = _mapper.Map(_source, _processDate, _paCode);

            Assert.Equal(_processDate, result.ProcessDate);
            Assert.Equal(_paCode, result.DealerCode);
        }

        [Fact]
        public void WhenMapping_It_MapsApprovedAmount()
        {
            var result = _mapper.Map(_source, _processDate, _paCode);

            var jobReconciliation = _source.JobReconciliation.First();
            Assert.Equal(jobReconciliation.ApprovedAmount.Value, result.ApprovedAmount.Value);
        }

        [Fact]
        public void WhenMapping_It_MapsStatusFromDispositionReason()
        {
            var result = _mapper.Map(_source, _processDate, _paCode);

            var dispositionReason = _source.JobReconciliation.First().DispositionReason.First();
            Assert.Equal(dispositionReason.DispositionStatusCode, result.Status.Code);
            Assert.Equal(dispositionReason.DispositionStatusString, result.Status.Description);
        }

        [Fact]
        public void WhenMapping_It_MapsExceptions()
        {
            var result = _mapper.Map(_source, _processDate, _paCode);

            var expectedExceptions = _source.JobReconciliation.First().DispositionReason.First().ExceptionCodes;
            var resultExceptions = result.Exceptions.ToList();

            Assert.Equal(expectedExceptions.Length, resultExceptions.Count);
            Assert.Equal(expectedExceptions[0].Code.Value, resultExceptions[0].Code);
            Assert.Equal(expectedExceptions[0].ExceptionText.Value, resultExceptions[0].Description);
        }

        [Fact]
        public void WhenMapping_It_SetsEmptyCollections()
        {
            var result = _mapper.Map(_source, _processDate, _paCode);

            Assert.Empty(result.Taxes);
            Assert.Empty(result.PartExpenses);
            Assert.Empty(result.LaborExpenses);
            Assert.Empty(result.MiscellaneousExpenses);
            Assert.Empty(result.Deductibles);
            Assert.Null(result.PartsAmount);
            Assert.Null(result.LaborAmount);
            Assert.Null(result.OtherAmount);
        }

        [Fact]
        public void WhenMapping_It_GeneratesId()
        {
            var result = _mapper.Map(_source, _processDate, _paCode);

            var repairOrderNumber = _source.DocumentID.Value;
            var statusCode = _source.JobReconciliation.First().DispositionReason.First().DispositionStatusCode;
            var expected = _processDate.ToString() + "-" + _paCode + "-" + repairOrderNumber + "-" + statusCode;

            Assert.Equal(expected, result.Id);
            Assert.Equal(expected, result.OemId);
        }
    }
}
