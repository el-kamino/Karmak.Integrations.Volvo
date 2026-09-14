using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared
{
    public class FusionContact {
        public string Title { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string HomePhone { get; set; }
        public string CellPhone { get; set; }
        public string WorkPhone { get; set; }
        public string Email { get; set; }
        public IList<FusionAddress> Addresses { get; set; }
        public string LicenseNumber { get; set; }
    }
}