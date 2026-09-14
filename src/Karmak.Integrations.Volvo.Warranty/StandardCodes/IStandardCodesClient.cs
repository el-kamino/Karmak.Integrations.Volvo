using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.StandardCodes
{
    public interface IStandardCodesClient
    {
        Task<IEnumerable<StandardCode>> FetchCodesAsync(StandardCodesSettings standardCodesSettings);
    }
}
