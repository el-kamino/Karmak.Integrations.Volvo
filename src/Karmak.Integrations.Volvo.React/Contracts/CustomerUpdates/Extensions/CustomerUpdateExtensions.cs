using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Extensions;

public static class CustomerUpdateExtensions
{
    public static Dictionary<string, string> EntityMetadata(this Data.CustomerUpdate update)
    {
        return new Dictionary<string, string>
        {
            [TelemetryKeys.EntityType] = EntityTypes.CustomerUpdate,
            [TelemetryKeys.CustomerUpdateVolvoPassIdentifier] = update.VolvoPassRewardID.ToString(),
            [TelemetryKeys.CustomerUpdateVINIdentifier] = string.Join(",\n", update.CurrentVINs),
        };
    }
}
