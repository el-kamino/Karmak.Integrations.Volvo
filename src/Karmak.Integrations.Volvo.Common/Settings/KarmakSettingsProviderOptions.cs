
using Azure.Core;

namespace Karmak.ELK.Core.KICQ.Api.Core.Settings;

public class KarmakSettingsProviderOptions
{
    public string Url { get; set; }
    public string SettingsServiceResourceId { get; set; }
    public TimeSpan? CacheTimeout { get; set; }
    public TokenCredential Credential { get; set; }
}
