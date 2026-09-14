using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IStandardCodesService
    {
        Task<IEnumerable<StandardCode>> FetchCodesAsync();
    }
}
