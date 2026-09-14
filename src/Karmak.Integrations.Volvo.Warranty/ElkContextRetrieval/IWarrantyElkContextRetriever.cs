using System.Threading.Tasks;
using Karmak.Integrations.Elk.Identity.Context;

namespace Karmak.Integrations.Volvo.Warranty.ElkContextRetrieval;

public interface IWarrantyElkContextRetriever
{
    Task<ElkContext> GetElkContextAsync(string paCode);
}
