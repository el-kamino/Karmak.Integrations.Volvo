using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public class RepairOrderDataEntity : ReactDataEntity<RepairOrderSnapshot>
    {
        public const string EntityTypeName = "RepairOrder";

        public override string EntityType => EntityTypeName;
        public override string EntityId => Entity.RepairOrderNumber;
    }
}
