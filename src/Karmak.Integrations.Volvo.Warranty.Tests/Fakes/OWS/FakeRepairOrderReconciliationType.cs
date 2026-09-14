using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeRepairOrderReconciliationType
    {
        private static readonly Faker<RepairOrderReconciliationType> Faker =
            new Faker<RepairOrderReconciliationType>().StrictMode(false)
                .RuleFor(v => v.DocumentID, faker => new IdentifierType { Value = faker.Random.AlphaNumeric(10) })
                .RuleFor(v => v.JobReconciliation, _ => new[] { FakeJobReconciliationExtended.Generate() })
                .RuleFor(v => v.WarrantyClaimReconciliation, _ => new[] { FakeWarrantyClaimReconciliationType.Generate() });
        public static RepairOrderReconciliationType Generate() => Faker.Generate();
        public static IEnumerable<RepairOrderReconciliationType> Generate(int count) => Faker.Generate(count);
    }
}
