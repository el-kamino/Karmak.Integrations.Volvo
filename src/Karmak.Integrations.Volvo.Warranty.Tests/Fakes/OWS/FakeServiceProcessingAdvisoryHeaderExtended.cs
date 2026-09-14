using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeServiceProcessingAdvisoryHeaderExtended
    {
        private static readonly Faker<ServiceProcessingAdvisoryHeaderExtended> Faker =
            new Faker<ServiceProcessingAdvisoryHeaderExtended>().StrictMode(false)
                .RuleFor(v => v.DealerParty, _ => FakePartyABIEType.Generate())
                .RuleFor(v => v.DispositionPayments, _ => new[] { FakeDispositionPaymentsExtended.Generate() });
        public static ServiceProcessingAdvisoryHeaderExtended Generate() => Faker.Generate();
        public static IEnumerable<ServiceProcessingAdvisoryHeaderExtended> Generate(int count) => Faker.Generate(count);
    }
}
