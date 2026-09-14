using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Extensions;

public static class PartsInventoryExtensions
{
    public static Dictionary<string, string> EntityMetadata(this PartsInventoryReport inventory)
    {
        return new Dictionary<string, string>
        {
            [TelemetryKeys.EntityType] = EntityTypes.PartsInventory,
            [TelemetryKeys.PartsInventoryIdentifier] = inventory.Id.ToString(),
            [TelemetryKeys.PartsInventoryType] = inventory.Type.ToString("G")
        };
    }
}
