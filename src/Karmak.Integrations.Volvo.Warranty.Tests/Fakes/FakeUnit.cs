using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeUnit
    {
        private static readonly Faker<Unit> Faker = new Faker<Unit>()
            .StrictMode(true)
            .RuleFor(f => f.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.ExternalIdentifiers, _ => new[] { FakeExternalIdentifier.Generate() })
            .RuleFor(f => f.Vehicle, FakeVehicle.Generate)
            .RuleFor(f => f.MeterReadings, faker => FakeMeterReading.Generate());

        public static Unit Generate() => Faker.Generate();
        public static IEnumerable<Unit> Generate(int count) => Faker.Generate(count);
    }
}
