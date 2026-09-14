using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Extensions;

public static class RepairOrderSnapshotExtensions
{
    public static Dictionary<string, string> EntityMetadata(this RepairOrderSnapshot order)
    {
        return new Dictionary<string, string>
        {
            [TelemetryKeys.EntityType] = EntityTypes.RepairOrder,
            [TelemetryKeys.RepairOrderIdentifier] = order.RepairOrderNumber,
            [TelemetryKeys.RepairOrderSnapshotId] = order.SnapshotId.ToString(),
        };
    }
}
