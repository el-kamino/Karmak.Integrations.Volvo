namespace Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0
{
    public class VolvoAddressWithPrivacy : VolvoAddress
    {
        public VolvoPrivacy privacy { get; set; }
    }

    public class VolvoAddress
    {
        public string lineOne { get; set; }
        public string lineTwo { get; set; }
        public string lineThree { get; set; }
        public string lineFour { get; set; }
        public string cityName { get; set; }
        public string countryId { get; set; }
        public string postcode { get; set; }
        public string stateOrProvinceCountrySubDivisionId { get; set; }
        public string countyCountrySubDivision { get; set; }
    }
}
