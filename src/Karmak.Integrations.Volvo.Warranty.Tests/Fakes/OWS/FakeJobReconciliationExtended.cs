using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeJobReconciliationExtended
    {
        private static readonly Faker<JobReconciliationExtended> Faker =
            new Faker<JobReconciliationExtended>().StrictMode(false)
                .RuleFor(v => v.JobNumberString, faker => faker.Random.AlphaNumeric(2))
                .RuleFor(v => v.ClaimTypeString, faker => faker.Random.AlphaNumeric(2))
                .RuleFor(v => v.ClaimStatusCode, faker => faker.Random.AlphaNumeric(2))
                .RuleFor(v => v.ScheduledDeleteDate, faker => faker.Date.Soon(14))
                .RuleFor(v => v.ClaimProcessingPeriod, faker => faker.Date.Recent(7))
                .RuleFor(v => v.ServiceParts, _ => new[] { FakeServicePartsType1.Generate() })
                .RuleFor(v => v.ServiceLabor, _ => new[] { FakeServiceLaborType1.Generate() })
                .RuleFor(v => v.ServiceComponents, _ => new[] { FakeServiceComponentsType1.Generate() })
                .RuleFor(v => v.Tax, _ => new[] { FakeTaxType.Generate() })
                .RuleFor(v => v.DispositionReason, _ => new[] { FakeDispositionReasonExtended.Generate() })
                .RuleFor(v => v.ApprovedAmount, faker => new AmountType { Value = faker.Random.Decimal(1, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() })
                .RuleFor(v => v.LaborAmount, faker => new AmountType { Value = faker.Random.Decimal(1, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() })
                .RuleFor(v => v.PartsAmount, faker => new AmountType { Value = faker.Random.Decimal(1, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() })
                .RuleFor(v => v.OtherAmount, faker => new AmountType { Value = faker.Random.Decimal(1, 1000), currencyID = faker.PickRandom<CurrencyCode>().ToString() });
        public static JobReconciliationExtended Generate() => Faker.Generate();
        public static IEnumerable<JobReconciliationExtended> Generate(int count) => Faker.Generate(count);
    }
}
