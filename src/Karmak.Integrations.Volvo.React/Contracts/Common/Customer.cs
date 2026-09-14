using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.Common
{
    public class Customer
    {
        public string CustomerKey { get; set; }
        public string CompanyName { get; set; }
        public string BusinessStructure { get; set; }
        public string CustomerTypeCode { get; set; }
        public string IndustryType { get; set; }
        public IList<Contact> Contacts { get; set; }
        public IList<Address> Addresses { get; set; }
        public IList<Phone> Phones { get; set; }
        public IList<ExternalIdentifier> ExternalIdentifiers { get; set; }

        public Customer()
        {
            Contacts = new List<Contact>();
            Addresses = new List<Address>();
            Phones = new List<Phone>();
            ExternalIdentifiers = new List<ExternalIdentifier>();
        }
    }
}
