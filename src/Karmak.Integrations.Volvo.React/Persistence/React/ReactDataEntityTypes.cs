using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    /// <summary>
    /// Every <c>EntityType</c> discriminator written to <c>dbo.ReactDataEntities</c>. Sourced from the
    /// <c>*DataEntity</c> constants so the set that gets cleaned up cannot drift from the set that gets written.
    /// </summary>
    public static class ReactDataEntityTypes
    {
        public static readonly IReadOnlyList<string> All =
        [
            RepairOrderDataEntity.EntityTypeName,
            CustomerUpdateDataEntity.EntityTypeName,
            PartsSalesOrderDataEntity.EntityTypeName,
            VehicleSalesOrderDataEntity.EntityTypeName
        ];
    }
}
