using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.Common
{
    public class Contact {
        public string Title { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public IList<Phone> Phones { get; set; }
        public string Email { get; set; }
        public IList<Address> Addresses { get; set; }
        public string LicenseNumber { get; set; }

        public Contact() {
            Phones = new List<Phone>();
            Addresses = new List<Address>();
        }
    }
}
