using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeMoney
    {
        private static readonly Faker<Money> Faker = new Faker<Money>()
            .StrictMode(true)
            .RuleFor(a => a.Currency, faker => faker.PickRandom<CurrencyCode>())
            .RuleFor(a => a.Value, faker => faker.Random.Decimal(1, 1000));

        public static Money Generate() => Faker.Generate();
        public static IEnumerable<Money> Generate(int count) => Faker.Generate(count);
    }
}
