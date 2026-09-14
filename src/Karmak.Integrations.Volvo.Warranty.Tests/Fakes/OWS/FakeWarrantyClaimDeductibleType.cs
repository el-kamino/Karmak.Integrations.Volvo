using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeWarrantyClaimDeductibleType
    {
        private static readonly Faker<WarrantyClaimDeductibleType> Faker =
            new Faker<WarrantyClaimDeductibleType>().StrictMode(false)
                .RuleFor(v => v.DeductibleAmount, faker => new AmountType { Value = faker.Random.Decimal(1, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() })
                .RuleFor(v => v.DeductibleTypeString, faker => faker.Random.AlphaNumeric(50));
        public static WarrantyClaimDeductibleType Generate() => Faker.Generate();
        public static IEnumerable<WarrantyClaimDeductibleType> Generate(int count) => Faker.Generate(count);
    }
}
