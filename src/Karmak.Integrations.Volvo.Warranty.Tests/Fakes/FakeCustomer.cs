using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeCustomer
    {
        private static readonly Faker<Customer> Faker = new Faker<Customer>()
            .StrictMode(true)
            .RuleFor(customer => customer.Identifier, faker => faker.Random.Uuid().ToString())
            .RuleFor(customer => customer.CompanyName, faker => faker.Company.CompanyName())
            .RuleFor(customer => customer.ExternalIdentifiers, _ => new[] { FakeExternalIdentifier.Generate() })
            .RuleFor(customer => customer.FirstName, faker => faker.Person.FirstName)
            .RuleFor(customer => customer.MiddleName, faker => faker.Random.Utf16String(1, 1))
            .RuleFor(customer => customer.LastName, faker => faker.Person.LastName)
            .RuleFor(customer => customer.WorkPhoneNumber, faker => faker.Phone.PhoneNumber())
            .RuleFor(customer => customer.CellPhoneNumber, faker => faker.Phone.PhoneNumber())
            .RuleFor(customer => customer.EmailAddress, faker => faker.Person.Email)
            .RuleFor(customer => customer.PhysicalAddress, FakeAddress.Generate)
            .RuleFor(customer => customer.PreferredContactMethod, faker => faker.PickRandom<ContactMethod>());

        public static Customer Generate() => Faker.Generate();
        public static IEnumerable<Customer> Generate(int count) => Faker.Generate(count);
    }
}
