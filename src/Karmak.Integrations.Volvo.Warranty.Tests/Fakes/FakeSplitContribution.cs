using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public class FakeSplitContribution
    {
        private static readonly Faker<SplitContribution> Faker = new Faker<SplitContribution>()
            .StrictMode(true)
            .RuleFor(split => split.CustomerAmount, FakeMoney.Generate)
            .RuleFor(split => split.DealerAmount, FakeMoney.Generate);

        public static SplitContribution Generate() => Faker.Generate();
        public static IEnumerable<SplitContribution> Generate(int count) => Faker.Generate(count);
    }
}
