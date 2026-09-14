using Bogus;
using Karmak.Integrations.Volvo.React.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeExternalIdentifier
    {
        private static readonly Faker<ExternalIdentifier> Faker = new Faker<ExternalIdentifier>()
            .StrictMode(true)
            .RuleFor(id => id.ID, faker => faker.Random.Uuid().ToString())
            .RuleFor(id => id.ExternalSourceType, faker => "FUSION");

        public static ExternalIdentifier Generate() => Faker.Generate();
        public static IEnumerable<ExternalIdentifier> Generate(int count) => Faker.Generate(count);
    }
}
