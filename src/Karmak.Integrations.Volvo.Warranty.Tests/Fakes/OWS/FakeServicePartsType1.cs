using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeServicePartsType1
    {
        private static readonly Faker<ServicePartsType1> Faker =
            new Faker<ServicePartsType1>().StrictMode(false)
                .RuleFor(v => v.ItemIdentification, faker => new IdentifierType { Value = faker.Random.AlphaNumeric(24) })
                .RuleFor(v => v.PartsReturnDestinationCode, faker => faker.Random.Decimal(0, 1000))
                .RuleFor(v => v.PartsReturnDestinationCodeSpecified, faker => faker.Random.Bool())
                .RuleFor(v => v.ItemQuantity, faker => new QuantityType { Value = faker.Random.Decimal(0, 1000), unitCode = faker.Random.AlphaNumeric(5) })
                .RuleFor(v => v.PartAmount, faker => new AmountType { Value = faker.Random.Decimal(0, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() });
        public static ServicePartsType1 Generate() => Faker.Generate();
        public static IEnumerable<ServicePartsType1> Generate(int count) => Faker.Generate(count);
    }
}
