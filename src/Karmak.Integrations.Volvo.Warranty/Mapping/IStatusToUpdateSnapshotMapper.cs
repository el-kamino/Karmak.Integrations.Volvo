using System;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Mapping
{
    public interface IStatusToUpdateSnapshotMapper
    {
        UpdateSnapshot Map(RepairOrderReconciliationType source, DateTime processDate, string paCode);
    }
}
