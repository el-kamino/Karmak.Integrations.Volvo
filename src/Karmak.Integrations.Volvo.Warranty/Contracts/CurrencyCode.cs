// ReSharper disable InconsistentNaming
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CurrencyCode
    {
        USD,
        CAD
    }
}
