using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeRepairOrder
    {
        private static readonly Faker<RepairOrder> Faker = new Faker<RepairOrder>()
            .StrictMode(true)
            .RuleFor(f => f.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.ExternalIdentifiers, _ => new[] { FakeExternalIdentifier.Generate() })
            .RuleFor(f => f.SecondaryIdentifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.SecondaryExternalIdentifiers, _ => new[] { FakeExternalIdentifier.Generate() })
            .RuleFor(f => f.Advisor, FakeUser.Generate)
            .RuleFor(f => f.Jobs, _ => FakeJob.Generate(1))
            .RuleFor(f => f.OpenedDate, faker => faker.Date.Past())
            .RuleFor(f => f.CompletedDate, faker => faker.Date.Past())
            .RuleFor(f => f.InvoicedDate, faker => faker.Date.Past())
            .RuleFor(f => f.InvoiceIdentifier, _ => Guid.NewGuid().ToString())
            .RuleFor(f => f.ApprovalIdentifiers, faker => faker.Make(faker.Random.Int(1, 4), faker.Random.Uuid().ToString))
            .RuleFor(f => f.IsExtendedServiceContractFranchise, faker => faker.Random.Bool());

        public static RepairOrder Generate() => Faker.Generate();
    }
}
