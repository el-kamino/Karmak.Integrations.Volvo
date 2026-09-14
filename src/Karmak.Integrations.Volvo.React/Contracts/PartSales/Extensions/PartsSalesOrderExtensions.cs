using System.Collections.Generic;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Karmak.Integrations.Volvo.React.Contracts.PartSales.Extensions;

public static class PartsSalesOrderExtensions
{
    public static Dictionary<string, string> EntityMetadata(this PartsSalesOrder order)
    {
        return new Dictionary<string, string>
        {
            [TelemetryKeys.EntityType] = EntityTypes.PartSalesOrder,
            [TelemetryKeys.PartsSalesInvoiceIdentifier] = order.InvoiceNumber,
        };
    }
}
