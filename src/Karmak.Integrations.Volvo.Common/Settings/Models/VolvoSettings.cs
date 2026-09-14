using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Common.Settings.Models;

public class VolvoSettings
{
    [JsonProperty("Volvo.DealerServiceProviderSettings")]
    public DealerServiceProviderSettings DealerServiceProviderSettings { get; set; }

    [JsonProperty("Volvo.RegionSettings")]
    public RegionSettings RegionSettings { get; set; }

    [JsonProperty("Volvo.InterfaceOptions")]
    public InterfaceOptions InterfaceOptions { get; set; }
}
