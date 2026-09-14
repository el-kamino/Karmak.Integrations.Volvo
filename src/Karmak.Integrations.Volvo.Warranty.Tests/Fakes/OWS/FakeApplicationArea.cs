using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS
{
    public static class FakeApplicationArea
    {
        private static readonly Faker<ApplicationAreaType> Faker =
            new Faker<ApplicationAreaType>().StrictMode(false)
                .RuleFor(v => v.Sender, faker => new SenderType
                {
                    ServiceID = new IdentifierType { Value = faker.Random.AlphaNumeric(10) }
                })
                .RuleFor(v => v.Destination, faker => new DestinationType
                {
                    DealerNumberID = new IdentifierType { Value = faker.Random.AlphaNumeric(5) + "!" }
                });
        public static ApplicationAreaType Generate() => Faker.Generate();
    }
}
