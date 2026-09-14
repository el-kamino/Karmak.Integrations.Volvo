using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeDriver
    {
        private static readonly Faker<Driver> Faker = new Faker<Driver>()
            .StrictMode(true)
            .RuleFor(driver => driver.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(driver => driver.FullName, faker => faker.Person.FullName);

        public static Driver Generate() => Faker.Generate();
        public static IEnumerable<Driver> Generate(int count) => Faker.Generate(count);
    }
}
