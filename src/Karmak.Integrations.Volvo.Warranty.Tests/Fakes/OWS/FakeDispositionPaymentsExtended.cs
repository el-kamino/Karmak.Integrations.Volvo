using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeDispositionPaymentsExtended
    {
        private static readonly Faker<DispositionPaymentsExtended> Faker =
            new Faker<DispositionPaymentsExtended>().StrictMode(false)
                .RuleFor(v => v.ProcessDate, faker => faker.Date.Between(DateTime.MinValue, DateTime.Now))
                .RuleFor(v => v.RepairOrderReconciliation, _ => new[] { FakeRepairOrderReconciliationType.Generate() });
        public static DispositionPaymentsExtended Generate() => Faker.Generate();
        public static IEnumerable<DispositionPaymentsExtended> Generate(int count) => Faker.Generate(count);
    }
}
