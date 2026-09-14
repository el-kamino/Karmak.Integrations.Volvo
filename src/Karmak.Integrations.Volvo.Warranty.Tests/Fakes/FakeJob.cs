using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeJob
    {
        private static readonly Faker<Job> Faker = new Faker<Job>().StrictMode(true)
            .RuleFor(f => f.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.ComplaintCode, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.CustomerNotes, faker => faker.Lorem.Paragraph())
            .RuleFor(f => f.TechnicianNotes, faker => faker.Lorem.Paragraph())
            .RuleFor(f => f.InternalDealerNotes, faker => faker.Lorem.Paragraph())
            .RuleFor(f => f.CampaignOptionCode, faker => faker.Random.Uuid().ToString())
            .RuleFor(f => f.LaborExpenses, faker => FakeLaborExpense.Generate(faker.Random.Int(1, 4)))
            .RuleFor(f => f.PartExpenses, faker => FakePartExpense.Generate(faker.Random.Int(1, 4)))
            .RuleFor(f => f.MiscellaneousExpenses, faker => FakeMiscellaneousExpense.Generate(faker.Random.Int(1, 4)))
            .RuleFor(f => f.Expenses, (faker, fake) => fake.LaborExpenses.Concat<Expense>(fake.PartExpenses).Concat(fake.MiscellaneousExpenses).ToList())
            .RuleFor(f => f.PreviousPartClaim, FakePreviousPartClaim.Generate)
            .RuleFor(f => f.Diagnostics, FakeDiagnostics.Generate)
            .RuleFor(f => f.Transportation, FakeTransportation.Generate)
            .RuleFor(f => f.AppealReasonCode, faker => faker.Random.AlphaNumeric(10))
            .RuleFor(f => f.AppealComments, faker => faker.Lorem.Paragraph());

        public static Job Generate() => Faker.Generate();
        public static IEnumerable<Job> Generate(int count) => Faker.Generate(count);
    }
}
