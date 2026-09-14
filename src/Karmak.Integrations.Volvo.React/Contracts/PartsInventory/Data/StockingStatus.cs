using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StockingStatus {
        Stocked,
        NonStocked,
        Unknown
    }
}