using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeMiscellaneousExpense
    {
        private static readonly Faker<MiscellaneousExpense> Faker = new Faker<MiscellaneousExpense>()
            .StrictMode(true)
            .RuleFor(f => f.Identifier, faker => faker.Random.Guid().ToString())
            .RuleFor(f => f.Description, faker => faker.Lorem.Sentence())
            .RuleFor(f => f.UnitPrice, faker => FakeMoney.Generate())
            .RuleFor(f => f.Quantity, FakeQuantity.Generate)
            .RuleFor(f => f.SecondQuantity, FakeQuantity.Generate)
            .RuleFor(f => f.ThirdPartyInvoiceIdentifier, faker => faker.Random.Guid().ToString())
            .RuleFor(f => f.AppealCode, faker => faker.PickRandom("A", "C", "D"));

        public static MiscellaneousExpense Generate() => Faker.Generate();
        public static IList<MiscellaneousExpense> Generate(int count) => Faker.Generate(count);
    }
}
