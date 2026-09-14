using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeShowServiceProcessingAdvisoryType
    {
        private static readonly Faker<ShowServiceProcessingAdvisoryType> Faker =
            new Faker<ShowServiceProcessingAdvisoryType>().StrictMode(false)
                .RuleFor(v => v.ShowServiceProcessingAdvisoryDataArea, _ => new ShowServiceProcessingAdvisoryDataAreaType
                {
                    ServiceProcessingAdvisory = new[] { FakeServiceProcessingAdvisoryType.Generate() }
                })
                .RuleFor(v => v.ApplicationArea, _ => FakeApplicationArea.Generate());
        public static ShowServiceProcessingAdvisoryType Generate() => Faker.Generate();
        public static IEnumerable<ShowServiceProcessingAdvisoryType> Generate(int count) => Faker.Generate(count);
    }
}
