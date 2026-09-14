using System.Threading;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IFusionClient
    {
        Task ProcessWarrantyPaymentAsync(WarrantyPaymentInformation payment, CancellationToken cancellationToken);
    }
}
