using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeLaborExpense
    {
        private static readonly Faker<LaborExpense> Faker = new Faker<LaborExpense>()
            .StrictMode(true)
            .RuleFor(f => f.Identifier, faker => faker.Random.Guid().ToString())
            .RuleFor(f => f.Description, faker => faker.Lorem.Sentence())
            .RuleFor(f => f.UnitPrice, faker => FakeMoney.Generate())
            .RuleFor(f => f.Quantity, FakeQuantity.Generate)
            .RuleFor(f => f.Technician, FakeUser.Generate)
            .RuleFor(f => f.ThirdPartyInvoiceIdentifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.IsSublet, fake => fake.Random.Bool())
            .RuleFor(f => f.AppealCode, faker => faker.PickRandom("A", "C", "D"));
        public static LaborExpense Generate() => Faker.Generate();
        public static IList<LaborExpense> Generate(int count) => Faker.Generate(count);
    }
}
