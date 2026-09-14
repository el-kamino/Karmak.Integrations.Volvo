using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Common.Settings.Models;

public class SettingsRecord
{
    public string DocumentId { get; set; }

    [JsonProperty("context_level")]
    public string ContextLevel { get; set; }

    [JsonProperty("context_id")]
    public string ContextId { get; set; }

    [JsonProperty("definition_id")]
    public string RecordDefinitionId { get; set; }

    [JsonProperty("data")]
    public string Data { get; set; }
}
