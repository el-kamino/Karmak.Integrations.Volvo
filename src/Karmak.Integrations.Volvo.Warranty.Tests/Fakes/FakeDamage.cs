using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public class FakeDamage
    {
        private static readonly Faker<Damage> Faker = new Faker<Damage>().StrictMode(true)
            .RuleFor(d => d.Area, faker => faker.Random.AlphaNumeric(2))
            .RuleFor(d => d.Type, faker => faker.Random.AlphaNumeric(2))
            .RuleFor(d => d.Severity, faker => faker.Random.AlphaNumeric(1));

        public static Damage Generate() => Faker.Generate();
    }
}
