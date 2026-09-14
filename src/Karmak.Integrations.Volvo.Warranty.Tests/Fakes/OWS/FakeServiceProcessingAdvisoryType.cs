using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeServiceProcessingAdvisoryType
    {
        private static readonly Faker<ServiceProcessingAdvisoryType> Faker =
            new Faker<ServiceProcessingAdvisoryType>().StrictMode(false)
                .RuleFor(v => v.ServiceProcessingAdvisoryHeader, _ => FakeServiceProcessingAdvisoryHeaderExtended.Generate());
        public static ServiceProcessingAdvisoryType Generate() => Faker.Generate();
        public static IEnumerable<ServiceProcessingAdvisoryType> Generate(int count) => Faker.Generate(count);
    }
}
