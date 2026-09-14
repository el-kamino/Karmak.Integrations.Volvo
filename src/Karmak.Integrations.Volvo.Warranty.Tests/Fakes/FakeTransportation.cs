using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeTransportation
    {
        private static readonly Faker<Transportation> Faker = new Faker<Transportation>()
            .StrictMode(true)
            .RuleFor(t => t.CarrierName, faker => faker.Company.CompanyName())
            .RuleFor(t => t.InvoiceIdentifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(t => t.ArrivalDate, faker => faker.Date.Past())
            .RuleFor(t => t.Damage, FakeDamage.Generate);

        public static Transportation Generate() => Faker.Generate();
        public static IEnumerable<Transportation> Generate(int count) => Faker.Generate(count);
    }
}
