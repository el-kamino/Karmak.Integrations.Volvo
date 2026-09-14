using Karmak.Integrations.Volvo.Oasis.Models;

namespace Karmak.Integrations.Volvo.Oasis.Services
{
    public interface IOasisDataProvider
    {
        Task<OasisResponseRest> GetDataAsync(OasisRequestRest request);
    }
}
