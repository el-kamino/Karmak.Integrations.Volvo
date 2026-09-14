using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Handlers
{
    public class ClaimCorrelationHandlerTest
    {
        private readonly IClaimsService _claimsService;
        private readonly ILogger<UpdateSnapshotToClaimCorrelationHandler> _logger;
        public ClaimCorrelationHandlerTest()
        {
            _claimsService = Substitute.For<IClaimsService>();
            _logger = Substitute.For<ILogger<UpdateSnapshotToClaimCorrelationHandler>>();
        }

        [Fact]
        public async Task WhenFindByRepairOrderIdReturnsMultipleWithDifferentCorrelationIds_It_ReturnsNull()
        {
            _claimsService.FindByRepairOrderId(Arg.Any<string>())
                .Returns(AutoFaker.Generate<Claim>(3));

            var correlationHandler = new UpdateSnapshotToClaimCorrelationHandler(_claimsService, _logger);

            var result = await correlationHandler.Correlate("12345");
            Assert.Null(result);
        }

        [Fact]
        public async Task WhenFindByRepairOrderIdReturnsMultipleWithMatchingCorrelationIds_It_ReturnsAll()
        {
            var faker = new Bogus.Faker<Claim>()
                .RuleFor(claim => claim.CorrelationId, "sameId");
            _claimsService.FindByRepairOrderId(Arg.Any<string>())
                .Returns(faker.Generate(3));

            var correlationHandler = new UpdateSnapshotToClaimCorrelationHandler(_claimsService, _logger);

            var result = await correlationHandler.Correlate("12345");
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.True(result.All(claim => claim.CorrelationId.Equals("sameId")));
        }

        [Fact]
        public async Task WhenFindByRepairOrderIdReturnsNone_It_ReturnsNull()
        {
            _claimsService.FindByRepairOrderId(Arg.Any<string>())
                .Returns(AutoFaker.Generate<Claim>(0));

            var correlationHandler = new UpdateSnapshotToClaimCorrelationHandler(_claimsService, _logger);

            var result = await correlationHandler.Correlate("12345");
            Assert.Null(result);
        }

        [Fact]
        public async Task WhenFindByRepairOrderIdReturnsSingle_It_ReturnsTheClaim()
        {
            var expectedClaim = AutoFaker.Generate<Claim>();
            _claimsService.FindByRepairOrderId(Arg.Any<string>())
                .Returns(new List<Claim>() { expectedClaim });

            var correlationHandler = new UpdateSnapshotToClaimCorrelationHandler(_claimsService, _logger);

            var actualClaim = await correlationHandler.Correlate("12345");
            Assert.Equal(expectedClaim.Id, actualClaim.First().Id);
        }
    }
}
