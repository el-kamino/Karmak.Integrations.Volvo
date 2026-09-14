using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Extensions;

public static class VehicleSalesOrderExtensions
{
    public static Dictionary<string, string> EntityMetadata(this VehicleSalesOrder order)
    {
        return new Dictionary<string, string>
        {
            [TelemetryKeys.EntityType] = EntityTypes.VehicleSale,
            [TelemetryKeys.VehicleSalesInvoiceIdentifier] = order.InvoiceNumber
        };
    }
}
