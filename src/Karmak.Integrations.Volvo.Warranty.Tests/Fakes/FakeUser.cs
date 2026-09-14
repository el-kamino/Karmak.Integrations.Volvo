using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeUser
    {
        private static readonly Faker<User> Faker = new Faker<User>()
            .StrictMode(true)
            .RuleFor(oemUser => oemUser.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.Username, faker => faker.Person.UserName);

        public static User Generate() => Faker.Generate();
        public static IEnumerable<User> Generate(int count) => Faker.Generate(count);
    }
}
