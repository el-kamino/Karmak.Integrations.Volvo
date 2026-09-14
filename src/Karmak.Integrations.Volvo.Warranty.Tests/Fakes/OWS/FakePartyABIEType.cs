using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakePartyABIEType
    {
        private static readonly Faker<PartyABIEType> Faker =
            new Faker<PartyABIEType>().StrictMode(false)
                .RuleFor(v => v.PartyID, faker => new IdentifierType { Value = faker.Random.AlphaNumeric(7) + "!" + faker.Random.AlphaNumeric(7) })
                .RuleFor(v => v.LocationID, faker => new IdentifierType { Value = faker.Random.AlphaNumeric(3) });
        public static PartyABIEType Generate() => Faker.Generate();
        public static IEnumerable<PartyABIEType> Generate(int count) => Faker.Generate(count);
    }
}
