using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class CountryCodeToCountryEnumeratedType : IConvertible<CountryCode, CountryEnumeratedType>
    {
        private static readonly IDictionary<CountryCode, CountryEnumeratedType> CountryCodeMappings = new Dictionary<CountryCode, CountryEnumeratedType>
        {
            [CountryCode.US] = CountryEnumeratedType.US,
            [CountryCode.CA] = CountryEnumeratedType.CA,
        };

        public CountryEnumeratedType Convert(CountryCode source) => CountryCodeMappings[source];
    }
}
