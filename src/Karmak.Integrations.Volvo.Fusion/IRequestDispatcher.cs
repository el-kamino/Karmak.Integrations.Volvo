using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Fusion
{
    public interface IRequestDispatcher
    {
        Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity);
    }
}
