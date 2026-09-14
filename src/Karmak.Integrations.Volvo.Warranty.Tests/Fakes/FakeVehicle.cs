using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeVehicle
    {
        private static readonly Faker<Vehicle> Faker = new Faker<Vehicle>()
            .StrictMode(true)
            .RuleFor(f => f.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.Year, faker => faker.Date.Past().Year.ToString())
            .RuleFor(f => f.Make, faker => faker.Vehicle.Manufacturer())
            .RuleFor(f => f.Model, faker => faker.Vehicle.Model())
            .RuleFor(f => f.InServiceDate, faker => faker.Date.Past())
            .RuleFor(f => f.License, FakeLicense.Generate)
            .RuleFor(f => f.SpecialUseIdentifier, faker => faker.PickRandom(SpecialUseIdentifierExamples));

        public static Vehicle Generate() => Faker.Generate();
        public static IEnumerable<Vehicle> Generate(int count) => Faker.Generate(count);

        private static readonly string[] SpecialUseIdentifierExamples = { "ambulance", "motor home", "funeral hearse" };
    }
}
