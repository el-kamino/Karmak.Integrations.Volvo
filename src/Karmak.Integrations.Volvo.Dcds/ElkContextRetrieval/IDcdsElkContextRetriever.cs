using Karmak.Integrations.Elk.Identity.Context;

namespace Karmak.Integrations.Volvo.Dcds.ElkContextRetrieval;

public interface IDcdsElkContextRetriever
{
    Task<ElkContext> GetElkContextAsync(string paCode);
}