using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IShowServiceProcessingAdvisoryHandler
    {
        Task<Result> HandleAsync(ShowServiceProcessingAdvisoryType request);
    }
}
