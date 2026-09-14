using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.Common.Settings
{
    public interface ISettingsProvider
    {
        Task<VolvoSettings> GetSettingsAsync();
        Task<KicqSettings> GetBridgeSettingsAsync();
        Task UpdateRecordAsync(SettingsRecord document);
    }
}
