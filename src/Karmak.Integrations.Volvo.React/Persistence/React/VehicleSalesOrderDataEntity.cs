using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public class VehicleSalesOrderDataEntity : ReactDataEntity<VehicleSalesOrder>
    {
        public const string EntityTypeName = "VehicleSalesOrder";

        public override string EntityType => EntityTypeName;
        public override string EntityId => Entity.InvoiceNumber;
    }
}
