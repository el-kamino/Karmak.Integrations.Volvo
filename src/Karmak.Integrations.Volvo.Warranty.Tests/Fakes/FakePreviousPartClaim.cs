using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakePreviousPartClaim
    {
        private static readonly Faker<PreviousPartClaim> Faker = new Faker<PreviousPartClaim>()
            .StrictMode(true)
            .RuleFor(sp => sp.InvoiceIdentifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(sp => sp.InvoiceDate, faker => faker.Date.Past())
            .RuleFor(sp => sp.Measurement, FakeMeasurement.GenerateMiles);

        public static PreviousPartClaim Generate() => Faker.Generate();
        public static IEnumerable<PreviousPartClaim> Generate(int count) => Faker.Generate(count);
    }
}
