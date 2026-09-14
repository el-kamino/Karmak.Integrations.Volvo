using AutoBogus;
using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeDealer
    {
        private static readonly Faker<Dealer> Faker = new Faker<Dealer>()
            .StrictMode(true)
            .RuleFor(dealer => dealer.Account, _ => AutoFaker.Generate<Account>())
            .RuleFor(dealer => dealer.Branch, _ => AutoFaker.Generate<Branch>())
            .RuleFor(dealer => dealer.InstanceIdentifier, _ => Guid.NewGuid().ToString());

        public static Dealer Generate() => Faker.Generate();
        public static IEnumerable<Dealer> Generate(int count) => Faker.Generate(count);
    }
}
