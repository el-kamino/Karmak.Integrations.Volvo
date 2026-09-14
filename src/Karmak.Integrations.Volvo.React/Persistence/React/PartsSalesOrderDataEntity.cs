using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public class PartsSalesOrderDataEntity : ReactDataEntity<PartsSalesOrder>
    {
        public const string EntityTypeName = "PartsSalesOrder";

        public override string EntityType => EntityTypeName;
        public override string EntityId => Entity.InvoiceNumber;
    }
}
