using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Elk.Identity.Retrieval;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Dcds.ElkContextRetrieval;

public class DcdsElkContextRetriever : IDcdsElkContextRetriever
{
    public const string ServiceKey = "DcdsContextRetriever";

    private readonly IContextClient _client;

    public DcdsElkContextRetriever([FromKeyedServices(ServiceKey)]IContextClient contextClient)
    {
        _client = contextClient;
    }

    public async Task<ElkContext> GetElkContextAsync(string paCode)
    {
        try
        {
            var lookupType = ContextLookupType.VolvoPACode(paCode);
            ElkContext result = await _client.GetAsync(lookupType);
            return result;
        }
        catch(Exception e)
        {
            throw new Exception($"Failed to lookup values for PA Code {paCode}", e);
        }
    }
}
