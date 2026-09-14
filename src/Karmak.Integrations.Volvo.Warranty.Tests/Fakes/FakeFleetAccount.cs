using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeFleetAccount
    {
        private static readonly Faker<FleetAccount> Faker = new Faker<FleetAccount>()
            .StrictMode(true)
            .RuleFor(f => f.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.PurchaseOrderIdentifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.PartsDiscountPercentage, faker => faker.Random.Decimal(1, 100))
            .RuleFor(f => f.LaborDiscountPercentage, faker => faker.Random.Decimal(1, 100))
            .RuleFor(f => f.MiscellaneousDiscountPercentage, faker => faker.Random.Decimal(1, 100))
            .RuleFor(f => f.ApprovedAmount, FakeMoney.Generate);

        public static FleetAccount Generate() => Faker.Generate();
        public static IEnumerable<FleetAccount> Generate(int count) => Faker.Generate(count);
    }
}
