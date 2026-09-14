using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeWarrantyClaimReconciliationType
    {
        private static readonly Faker<WarrantyClaimReconciliationType> Faker =
            new Faker<WarrantyClaimReconciliationType>().StrictMode(false)
                .RuleFor(v => v.ToBePaidAmount, faker => new AmountType { Value = faker.Random.Decimal(1, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() })
                .RuleFor(v => v.ClaimNumberString, faker => faker.Random.AlphaNumeric(10))
                .RuleFor(v => v.WarrantyClaimDeductible, _ => new WarrantyClaimDeductibleType[] { FakeWarrantyClaimDeductibleType.Generate() });
        public static WarrantyClaimReconciliationType Generate() => Faker.Generate();
        public static IEnumerable<WarrantyClaimReconciliationType> Generate(int count) => Faker.Generate(count);
    }
}
