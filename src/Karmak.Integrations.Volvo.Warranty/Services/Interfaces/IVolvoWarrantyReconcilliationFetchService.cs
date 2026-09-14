using System.Threading.Tasks;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IVolvoWarrantyReconcilliationFetchService
    {
        Task<(bool success, SoapResult response)> TriggerReconciliationFetch(
            ReconciliationFetchRequest dateRange);
    }
}
