using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeDispositionReasonExtended
    {
        private static readonly Faker<DispositionReasonExtended> Faker =
            new Faker<DispositionReasonExtended>().StrictMode(false)
                .RuleFor(v => v.DispositionStatusCode, faker => faker.Random.AlphaNumeric(3))
                .RuleFor(v => v.DispositionStatusString, faker => faker.Random.AlphaNumeric(64))
                .RuleFor(v => v.ExceptionCodes, _ => new[] { FakeExceptionCodes.Generate() });
        public static DispositionReasonExtended Generate() => Faker.Generate();
        public static IEnumerable<DispositionReasonExtended> Generate(int count) => Faker.Generate(count);
    }
}
