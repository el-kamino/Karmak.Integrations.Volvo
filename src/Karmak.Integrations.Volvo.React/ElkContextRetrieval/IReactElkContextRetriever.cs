using Karmak.Integrations.Elk.Identity.Context;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.ElkContextRetrieval;

public interface IReactElkContextRetriever
{
    Task<ElkContext> GetElkContextAsync(string paCode);
}