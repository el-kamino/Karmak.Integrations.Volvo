using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Karmak.Integrations.Volvo.React.Contracts.Common
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OdometerUnitType {
        MILES,
        KILOMETERS,
        UNKNOWN
    }
}
