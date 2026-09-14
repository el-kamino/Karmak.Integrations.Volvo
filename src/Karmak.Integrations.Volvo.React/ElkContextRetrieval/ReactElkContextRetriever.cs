using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Elk.Identity.Retrieval;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.ElkContextRetrieval;

public class ReactElkContextRetriever : IReactElkContextRetriever
{
    public const string ServiceKey = "ReactContextRetriever";

    private readonly IContextClient _client;

    public ReactElkContextRetriever([FromKeyedServices(ServiceKey)] IContextClient contextClient)
    {
        _client = contextClient;
    }

    public async Task<ElkContext> GetElkContextAsync(string paCode)
    {
        try
        {
            var contextLookup = ContextLookupType.VolvoPACode(paCode);
            ElkContext result = await _client.GetAsync(contextLookup);
            return result;
        }
        catch(Exception e)
        {
            throw new Exception($"Failed to lookup values for PA Code {paCode}", e);
        }
    }
}
