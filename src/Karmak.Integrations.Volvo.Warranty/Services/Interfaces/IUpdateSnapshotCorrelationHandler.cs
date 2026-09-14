using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IUpdateSnapshotCorrelationHandler
    {
        Task<IEnumerable<Claim>> Correlate(string repairOrderId);
    }
}
