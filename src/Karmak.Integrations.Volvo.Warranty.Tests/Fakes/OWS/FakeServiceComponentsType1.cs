using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeServiceComponentsType1
    {
        private static readonly Faker<ServiceComponentsType1> Faker = new Faker<ServiceComponentsType1>().StrictMode(false)
            .RuleFor(v => v.ServiceComponentAmount, faker => new AmountType { Value = faker.Random.Decimal(1, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() })
            .RuleFor(v => v.ComponentTypeCode, faker => new CodeType { Value = faker.Random.AlphaNumeric(2), name = faker.Random.AlphaNumeric(6) });
        public static ServiceComponentsType1 Generate() => Faker.Generate();
        public static IEnumerable<ServiceComponentsType1> Generate(int count) => Faker.Generate(count);
    }
}
