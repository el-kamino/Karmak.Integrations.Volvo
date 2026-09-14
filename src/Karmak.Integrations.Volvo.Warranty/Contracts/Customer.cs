using System.Collections.Generic;
using Karmak.Integrations.Volvo.React.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Customer
    {
        public string Identifier { get; set; }
        public IList<ExternalIdentifier> ExternalIdentifiers { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string CellPhoneNumber { get; set; }
        public string WorkPhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public Address PhysicalAddress { get; set; }
        public ContactMethod PreferredContactMethod { get; set; }
        public string FullName => string.IsNullOrEmpty(MiddleName) ?
            string.Join(" ", FirstName, LastName).Trim()
            : string.Join(" ", FirstName, MiddleName, LastName).Trim();
    }
}
