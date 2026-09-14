using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakePartExpense
    {
        private static readonly Faker<PartExpense> Faker = new Faker<PartExpense>()
            .StrictMode(true)
            .RuleFor(p => p.Identifier, faker => faker.Random.Guid().ToString())
            .RuleFor(p => p.Prefix, faker => faker.Random.Guid().ToString())
            .RuleFor(p => p.Number, faker => faker.Random.Guid().ToString())
            .RuleFor(p => p.Suffix, faker => faker.Random.Guid().ToString())
            .RuleFor(p => p.ConditionCode, faker => faker.Random.Guid().ToString())
            .RuleFor(p => p.ThirdPartyInvoiceIdentifier, faker => faker.Random.Guid().ToString())
            .RuleFor(p => p.Description, faker => faker.Lorem.Sentence())
            .RuleFor(p => p.UnitPrice, faker => FakeMoney.Generate())
            .RuleFor(p => p.UnitCost, faker => FakeMoney.Generate())
            .RuleFor(p => p.CorePrice, faker => FakeMoney.Generate())
            .RuleFor(p => p.ExtendedPrice, faker => FakeMoney.Generate())
            .RuleFor(p => p.Total, faker => FakeMoney.Generate())
            .RuleFor(p => p.Quantity, FakeQuantity.Generate)
            .RuleFor(p => p.IsCausalPart, faker => faker.Random.Bool())
            .RuleFor(f => f.AppealCode, faker => faker.PickRandom("A", "C", "D"));

        public static PartExpense Generate() => Faker.Generate();
        public static IList<PartExpense> Generate(int count) => Faker.Generate(count);
    }
}
