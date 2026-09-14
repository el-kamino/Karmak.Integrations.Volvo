using System;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.Shared.V5_14_4
{
    public static class CountryEvaluator
    {
        public static string ToAlpha3(string country)
        {
            if (string.IsNullOrEmpty(country))
            {
                return null;
            }
            var cleaned = country.Replace(".", "").ToLower();
            CountryInfoList.CountryInfo countryInfo = null;

            switch (cleaned.Length)
            {
                case 2:
                    CountryInfoList.CountriesByAlpha2().TryGetValue(cleaned, out countryInfo);
                    break;
                case 3:
                    CountryInfoList.CountriesByAlpha3().TryGetValue(cleaned, out countryInfo);
                    break;
                default:
                    CountryInfoList.CountriesByName().TryGetValue(cleaned, out countryInfo);
                    break;
            }

            return countryInfo?.Alpha3;
        }

        public static CountryEnumeratedType ToEnumeratedCountry(string country)
        {
            Enum.TryParse<CountryEnumeratedType>(ToAlpha3(country), out var countryCode);
            return countryCode;
        }
    }
}