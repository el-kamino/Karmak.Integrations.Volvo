using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeAddress
    {
        private static readonly Faker<Address> Faker = new Faker<Address>()
            .StrictMode(true)
            .RuleFor(address => address.StreetAddress, faker => faker.Address.StreetAddress())
            .RuleFor(address => address.SecondaryAddress, faker => faker.Address.SecondaryAddress())
            .RuleFor(address => address.City, faker => faker.Address.City())
            .RuleFor(address => address.Region, faker => faker.Address.State())
            .RuleFor(address => address.Country, faker => faker.PickRandom<CountryCode>())
            .RuleFor(address => address.PostalCode, faker => faker.Address.ZipCode())
            .RuleFor(address => address.County, faker => faker.Address.County());

        public static Address Generate() => Faker.Generate();
        public static IEnumerable<Address> Generate(int count) => Faker.Generate(count);
    }
}
