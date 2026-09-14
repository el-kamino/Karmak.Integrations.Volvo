using Karmak.Integrations.Volvo.Common.Sql.Models;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public class CustomerUpdateDataEntity : ReactDataEntity<Contracts.CustomerUpdates.Data.CustomerUpdate>
    {
        public const string EntityTypeName = "CustomerUpdate";

        public override string EntityType => EntityTypeName;

        //No entity id is used for customer updates
        public override string EntityId => null;
    }
}
