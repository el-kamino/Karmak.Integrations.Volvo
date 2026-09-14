using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeLicense
    {
        private static readonly Faker<License> Faker = new Faker<License>()
            .StrictMode(true)
            .RuleFor(license => license.Number, faker => faker.Random.AlphaNumeric(6))
            .RuleFor(license => license.State, faker => faker.Address.StateAbbr());

        public static License Generate() => Faker.Generate();
        public static IEnumerable<License> Generate(int count) => Faker.Generate(count);
    }
}
