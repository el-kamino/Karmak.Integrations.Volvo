using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public class FakeCompany
    {
        private static readonly Faker<Company> Faker = new Faker<Company>()
            .StrictMode(true)
            .RuleFor(address => address.Name, faker => faker.Company.CompanyName());

        public static Company Generate() => Faker.Generate();
        public static IEnumerable<Company> Generate(int count) => Faker.Generate(count);
    }
}
