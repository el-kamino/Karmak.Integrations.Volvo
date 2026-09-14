using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeQuantity
    {
        private static readonly Faker<Quantity> DefaultFaker = new Faker<Quantity>()
            .StrictMode(true)
            .RuleFor(q => q.Type, _ => UnitOfMeasureType.None)
            .RuleFor(q => q.Value, faker => faker.Random.Decimal(1, 1000));

        public static Quantity Generate() => DefaultFaker.Generate();
        public static IEnumerable<Quantity> Generate(int count) => DefaultFaker.Generate(count);
    }
}
