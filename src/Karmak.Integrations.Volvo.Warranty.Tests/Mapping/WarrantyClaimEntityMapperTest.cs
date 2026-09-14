using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Mapping
{
    public class WarrantyClaimEntityMapperTest
    {
        private readonly WarrantyClaimEntityMapper _mapper = new();

        #region ToEntity()

        [Fact]
        public void WhenMapping_It_PromotesTheSearchableFields()
        {
            var claim = FakeClaim.Generate();

            var entity = _mapper.ToEntity(claim);

            Assert.Equal(claim.CausalPartIdentifier, entity.CausalPartIdentifier);
            Assert.Equal(claim.Identifier, entity.ClaimIdentifier);
            Assert.Equal(claim.Customer.CompanyName, entity.CompanyName);
            Assert.Equal(claim.Total.Value, entity.ClaimTotal);
            Assert.Equal(claim.RepairOrder.CompletedDate, entity.RepairOrderCompletedDate);
            Assert.Equal(claim.RepairOrder.OpenedDate, entity.RepairOrderOpenedDate);
            Assert.Equal(claim.Customer.Identifier, entity.CustomerIdentifier);
            Assert.Equal(claim.RepairOrder.InvoiceIdentifier, entity.InvoiceIdentifier);
            Assert.Equal(claim.Oem.Name, entity.Oem);
            Assert.Equal(claim.RepairOrder.Identifier, entity.RepairOrderIdentifier);
            Assert.Equal(claim.Status.Value, entity.ClaimStatus);
            Assert.Equal(claim.Unit.Vehicle.Identifier, entity.VehicleIdentifier);
        }

        [Fact]
        public void WhenMapping_It_PromotesTheColumnsTheLookupsKeyOn()
        {
            var claim = FakeClaim.Generate();
            claim.CorrelationId = "correlation-1";
            claim.IsDeleted = true;

            var entity = _mapper.ToEntity(claim);

            Assert.Equal(claim.Id, entity.ClaimId);
            Assert.Equal(claim.Dealer.InstanceIdentifier, entity.InstanceIdentifier);
            Assert.Equal(claim.Dealer.Branch.Identifier, entity.BranchIdentifier);
            Assert.Equal(claim.Dealer.Branch.Code, entity.BranchCode);
            Assert.Equal(claim.RepairOrder.SecondaryIdentifier, entity.WarrantyRepairOrderIdentifier);
            Assert.Equal("correlation-1", entity.CorrelationId);
            Assert.True(entity.IsDeleted);
        }

        [Fact]
        public void WhenMappingAClaimWithNoRelations_It_LeavesThePromotedColumnsNull()
        {
            var claim = new Claim { Id = "claim-1" };

            var entity = _mapper.ToEntity(claim);

            Assert.Equal("claim-1", entity.ClaimId);
            Assert.Null(entity.InstanceIdentifier);
            Assert.Null(entity.BranchIdentifier);
            Assert.Null(entity.BranchCode);
            Assert.Null(entity.CompanyName);
            Assert.Null(entity.Oem);
            Assert.Null(entity.RepairOrderIdentifier);
            Assert.Null(entity.RepairOrderOpenedDate);
            Assert.Null(entity.RepairOrderCompletedDate);
            Assert.Null(entity.VehicleIdentifier);
            Assert.NotNull(entity.JsonData);
        }

        [Fact]
        public void WhenMappingANullClaim_It_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.ToEntity(null));
        }

        #endregion

        #region Round trip

        /// <summary>
        /// The claim graph leans on Newtonsoft: an id property remapped to "id", string enum
        /// converters, and types whose setters are not public. If the serializer ever drifts,
        /// this is what catches it.
        /// </summary>
        [Fact]
        public void WhenRoundTripping_It_PreservesTheClaim()
        {
            var claim = FakeClaim.Generate();
            claim.CorrelationId = "correlation-1";

            var result = _mapper.ToClaim(_mapper.ToEntity(claim));

            Assert.NotNull(result);
            Assert.Equal(claim.Id, result.Id);
            Assert.Equal(claim.CorrelationId, result.CorrelationId);
            Assert.Equal(claim.Identifier, result.Identifier);
            Assert.Equal(claim.Type, result.Type);
            Assert.Equal(claim.SubCode, result.SubCode);
            Assert.Equal(claim.IsDeleted, result.IsDeleted);
            Assert.Equal(claim.IsManualReviewRequired, result.IsManualReviewRequired);
            Assert.Equal(claim.ShouldHoldAtPreValidation, result.ShouldHoldAtPreValidation);
            Assert.Equal(claim.CreatedBy, result.CreatedBy);
            Assert.Equal(claim.CreatedDateTime, result.CreatedDateTime);
            Assert.Equal(claim.UpdatedBy, result.UpdatedBy);
            Assert.Equal(claim.UpdatedDateTime, result.UpdatedDateTime);
        }

        /// <summary>
        /// <see cref="ClaimStatus"/> has a private parameterless constructor and non-public
        /// setters, so it only survives the trip because Newtonsoft is doing the work.
        /// </summary>
        [Fact]
        public void WhenRoundTripping_It_PreservesTheClaimStatus()
        {
            var claim = FakeClaim.Generate();
            claim.Status = new ClaimStatus("In Process", "IP");

            var result = _mapper.ToClaim(_mapper.ToEntity(claim));

            Assert.NotNull(result.Status);
            Assert.Equal("In Process", result.Status.Value);
            Assert.Equal("IP", result.Status.Code);
        }

        [Fact]
        public void WhenRoundTripping_It_PreservesStringBackedEnums()
        {
            var claim = FakeClaim.Generate();
            claim.Unit.MeterReadings = new List<MeterReading>
            {
                new() { Type = MeterReadingType.EngineHours }
            };

            var entity = _mapper.ToEntity(claim);
            var result = _mapper.ToClaim(entity);

            //Serialized by name, not by ordinal, so the json stays readable and the value does
            //not shift if the enum is ever reordered
            Assert.Contains("EngineHours", entity.JsonData);
            Assert.Equal(MeterReadingType.EngineHours, result.Unit.MeterReadings.Single().Type);
        }

        [Fact]
        public void WhenRoundTripping_It_PreservesTheDealerAndRepairOrder()
        {
            var claim = FakeClaim.Generate();

            var result = _mapper.ToClaim(_mapper.ToEntity(claim));

            Assert.Equal(claim.Dealer.InstanceIdentifier, result.Dealer.InstanceIdentifier);
            Assert.Equal(claim.Dealer.Branch.Identifier, result.Dealer.Branch.Identifier);
            Assert.Equal(claim.Dealer.Account.Code, result.Dealer.Account.Code);
            Assert.Equal(claim.RepairOrder.Identifier, result.RepairOrder.Identifier);
            Assert.Equal(claim.RepairOrder.SecondaryIdentifier, result.RepairOrder.SecondaryIdentifier);
            Assert.Equal(claim.RepairOrder.InvoiceIdentifier, result.RepairOrder.InvoiceIdentifier);
            Assert.Equal(claim.RepairOrder.OpenedDate, result.RepairOrder.OpenedDate);
            Assert.Equal(claim.RepairOrder.CompletedDate, result.RepairOrder.CompletedDate);
            Assert.Equal(claim.RepairOrder.Jobs.Count(), result.RepairOrder.Jobs.Count());
        }

        /// <summary>
        /// CausalPartIdentifier is computed off the strongly typed part expenses, which do
        /// survive the trip intact.
        /// </summary>
        [Fact]
        public void WhenRoundTripping_It_PreservesTypedExpenses()
        {
            var claim = FakeClaim.Generate();
            var job = claim.RepairOrder.Jobs.First();
            var part = job.PartExpenses.First();
            part.IsCausalPart = true;

            var result = _mapper.ToClaim(_mapper.ToEntity(claim));
            var resultPart = result.RepairOrder.Jobs.First().PartExpenses.First();

            Assert.Equal(job.PartExpenses.Count, result.RepairOrder.Jobs.First().PartExpenses.Count);
            Assert.Equal(part.Identifier, resultPart.Identifier);
            Assert.Equal(part.CorePrice.Value, resultPart.CorePrice.Value);
            Assert.Equal(part.UnitPrice.Value, resultPart.UnitPrice.Value);
            Assert.Equal(part.Total.Value, resultPart.Total.Value);
            Assert.Equal(claim.CausalPartIdentifier, result.CausalPartIdentifier);
        }

        /// <summary>
        /// Job.Expenses is declared as the base type, so json carries no record of which
        /// subclass each element was and they come back as plain expenses. Claim.Total is
        /// summed over that list, so it shifts by the core prices a PartExpense would have
        /// added. This is what cosmos does too - its default serializer is Newtonsoft with the
        /// same settings - so the sql store reproduces it rather than introducing it, and
        /// migrated claims keep the totals they already had. Pinned here so a future change to
        /// the payload serializer has to be a deliberate one.
        /// </summary>
        [Fact]
        public void WhenRoundTripping_It_CollapsesBaseTypedExpensesTheSameWayCosmosDoes()
        {
            var claim = FakeClaim.Generate();

            var result = _mapper.ToClaim(_mapper.ToEntity(claim));

            Assert.All(
                result.RepairOrder.Jobs.SelectMany(job => job.Expenses),
                expense => Assert.Equal(typeof(Expense), expense.GetType()));

            var expectedTotal = claim.RepairOrder.Jobs
                .SelectMany(job => job.Expenses)
                .Sum(expense => expense.UnitPrice.Value * expense.Quantity.Value);

            Assert.Equal(expectedTotal, result.Total.Value);
        }

        /// <summary>
        /// The column promoted for searching is taken from the claim on the way in, so it
        /// records the total as it stood when the claim was written.
        /// </summary>
        [Fact]
        public void WhenMapping_It_PromotesTheTotalAsItStoodOnTheClaim()
        {
            var claim = FakeClaim.Generate();

            var entity = _mapper.ToEntity(claim);

            Assert.Equal(claim.Total.Value, entity.ClaimTotal);
        }

        [Fact]
        public void WhenRoundTripping_It_PreservesUpdateSnapshots()
        {
            var claim = FakeClaim.Generate();
            claim.UpdateSnapshots = new List<UpdateSnapshot>
            {
                new() { OemId = "oem-1", Type = UpdateType.Status, Status = new UpdateStatus { Code = "S1", Description = "Submitted" } }
            };

            var result = _mapper.ToClaim(_mapper.ToEntity(claim));

            var snapshot = Assert.Single(result.UpdateSnapshots);
            Assert.Equal("oem-1", snapshot.OemId);
            Assert.Equal(UpdateType.Status, snapshot.Type);
            Assert.Equal("Submitted", snapshot.Status.Description);
        }

        #endregion

        #region ToSearchResult()

        /// <summary>
        /// A result list shows the branch the way a dealer names it, so the code travels with the
        /// summary. The identifier does too, but only because it is what the search was scoped by.
        /// </summary>
        [Fact]
        public void WhenMappingASummary_It_ReturnsWhatAResultListShows()
        {
            var summary = new WarrantyClaimSummary
            {
                ClaimId = "claim-1",
                BranchIdentifier = "branch-1",
                BranchCode = "01",
                ClaimIdentifier = "C-1001",
                CompanyName = "ACME",
                ClaimTotal = 100.50m,
                ClaimStatus = "New"
            };

            var result = _mapper.ToSearchResult(summary);

            Assert.Equal("claim-1", result.ClaimId);
            Assert.Equal("branch-1", result.BranchIdentifier);
            Assert.Equal("01", result.BranchCode);
            Assert.Equal("C-1001", result.ClaimIdentifier);
            Assert.Equal("ACME", result.CompanyName);
            Assert.Equal(100.50m, result.ClaimTotal);
            Assert.Equal("New", result.ClaimStatus);
        }

        [Fact]
        public void WhenMappingANullSummary_It_ReturnsNull()
        {
            Assert.Null(_mapper.ToSearchResult(null));
        }

        #endregion

        #region ToClaim()

        [Fact]
        public void WhenMappingANullEntity_It_ReturnsNull()
        {
            Assert.Null(_mapper.ToClaim(null));
        }

        [Fact]
        public void WhenMappingAnEntityWithNoPayload_It_ReturnsNull()
        {
            Assert.Null(_mapper.ToClaim(new WarrantyClaimEntity { ClaimId = "claim-1" }));
        }

        #endregion
    }
}
