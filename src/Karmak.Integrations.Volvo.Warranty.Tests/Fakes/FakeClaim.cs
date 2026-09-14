using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeClaim
    {
        private static readonly Faker<Claim> Faker = new Faker<Claim>()
            .StrictMode(false)
            .RuleFor(claim => claim.CorrelationId, _ => null)
            .RuleFor(claim => claim.Id, faker => faker.Random.Uuid().ToString())
            .RuleFor(claim => claim.Oem, FakeCompany.Generate)
            .RuleFor(claim => claim.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(claim => claim.Status, _ => ClaimStatus.New)
            .RuleFor(claim => claim.Type, faker => faker.Random.Uuid().ToString())
            .RuleFor(claim => claim.SubCode, faker => faker.Random.Uuid().ToString())
            .RuleFor(claim => claim.RepairOrder, FakeRepairOrder.Generate)
            .RuleFor(claim => claim.Dealer, FakeDealer.Generate)
            .RuleFor(claim => claim.Customer, FakeCustomer.Generate)
            .RuleFor(claim => claim.Driver, FakeDriver.Generate)
            .RuleFor(claim => claim.Unit, FakeUnit.Generate)
            .RuleFor(claim => claim.FleetAccount, FakeFleetAccount.Generate)
            .RuleFor(claim => claim.Split, FakeSplitContribution.Generate)
            .RuleFor(claim => claim.IsManualReviewRequired, faker => faker.Random.Bool())
            .RuleFor(claim => claim.IsRelatedDamageIncluded, faker => faker.Random.Bool())
            .RuleFor(claim => claim.PreAuthorizationIdentifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(claim => claim.ShouldHoldAtPreValidation, faker => faker.Random.Bool())
            .RuleFor(claim => claim.CreatedBy, faker => faker.Person.FullName)
            .RuleFor(claim => claim.CreatedDateTime, faker => faker.Date.Recent())
            .RuleFor(claim => claim.UpdatedBy, faker => faker.Person.FullName)
            .RuleFor(claim => claim.UpdatedDateTime, faker => faker.Date.Recent());

        public static Claim Generate() => Faker.Generate();
        public static IEnumerable<Claim> Generate(int count) => Faker.Generate(count);
        public static IEnumerable<Claim> GenerateCorrelatedClaims(int count)
        {
            var claims = Generate(count);

            var correlationId = System.Guid.NewGuid().ToString();
            foreach (var claim in claims)
            {
                claim.CorrelationId = correlationId;
            }
            return claims;
        }
    }
}
