using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.Mappers.Shared.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Mappers.Shared._6_0_0
{
    internal class VolvoAddressBuilder
    {
        public static VolvoAddressWithPrivacy BuildAddress(Address address)
        {
            if (address == null)
                return null;

            return new VolvoAddressWithPrivacy
            {
                lineOne = string.IsNullOrWhiteSpace(address.Street1) ? null : address.Street1.WafSanitize().MaxLength(80),
                lineTwo = string.IsNullOrWhiteSpace(address.Street2) ? null : address.Street2.WafSanitize().MaxLength(80),
                cityName = string.IsNullOrWhiteSpace(address.City) ? null : address.City.WafSanitize().MaxLength(40),
                postcode = string.IsNullOrWhiteSpace(address.PostalCode) ? null : address.PostalCode.WafSanitize().MaxLength(20),
                countyCountrySubDivision = string.IsNullOrWhiteSpace(address.County) ? null : address.County.WafSanitize().MaxLength(30),
                stateOrProvinceCountrySubDivisionId = string.IsNullOrWhiteSpace(address.Region) ? null : address.Region.WafSanitize().MaxLength(30),
                countryId = Get3CharCountryCode(address.CountryCode)
            };
        }

        private static string Get3CharCountryCode(string countryCode)
        {
            var code = CountryEvaluator.ToAlpha3(countryCode);
            return string.IsNullOrWhiteSpace(code) 
                ? "USA" 
                : code;
        }
    }
}
