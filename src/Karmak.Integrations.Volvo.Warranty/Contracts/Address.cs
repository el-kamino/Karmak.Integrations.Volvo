namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Address
    {
        public string StreetAddress { get; set; }
        public string SecondaryAddress { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string County { get; set; }
        public string PostalCode { get; set; }
        public CountryCode Country { get; set; }
    }
}
