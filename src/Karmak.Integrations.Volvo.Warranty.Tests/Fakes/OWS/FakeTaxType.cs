using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeTaxType
    {
        private static readonly Faker<TaxType> Faker =
            new Faker<TaxType>().StrictMode(false)
                .RuleFor(v => v.TaxAmount, faker => new AmountType { Value = faker.Random.Decimal(1, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() })
                .RuleFor(v => v.TaxDescription, faker => new[] { new TextType { Value = faker.Random.AlphaNumeric(22) } })
                .RuleFor(v => v.TaxRatePercent, faker => faker.Random.Decimal(0, 100));
        public static TaxType Generate() => Faker.Generate();
        public static IEnumerable<TaxType> Generate(int count) => Faker.Generate(count);
    }
}
