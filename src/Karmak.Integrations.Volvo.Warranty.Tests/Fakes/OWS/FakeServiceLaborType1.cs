using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeServiceLaborType1
    {
        private static readonly Faker<ServiceLaborType1> Faker =
            new Faker<ServiceLaborType1>().StrictMode(false)
                .RuleFor(v => v.LaborActualHoursNumeric, faker => faker.Random.Decimal(0, 1000))
                .RuleFor(v => v.LaborOperationID, faker => new IdentifierType { Value = faker.Random.AlphaNumeric(11) })
                .RuleFor(v => v.LaborAmount, faker => new AmountType { Value = faker.Random.Decimal(0, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() });
        public static ServiceLaborType1 Generate() => Faker.Generate();
        public static IEnumerable<ServiceLaborType1> Generate(int count) => Faker.Generate(count);
    }
}
