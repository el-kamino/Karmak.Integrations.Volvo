using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeExceptionCodes
    {
        private static readonly Faker<ExceptionCodesType> Faker =
            new Faker<ExceptionCodesType>().StrictMode(false)
            .RuleFor(v => v.Code, faker => new CodeType { Value = faker.Random.AlphaNumeric(2), name = faker.Random.AlphaNumeric(6) })
            .RuleFor(v => v.ExceptionText, faker => new TextType { Value = faker.Random.AlphaNumeric(2000) });
        public static ExceptionCodesType Generate() => Faker.Generate();
        public static IEnumerable<ExceptionCodesType> Generate(int count) => Faker.Generate(count);
    }
}
