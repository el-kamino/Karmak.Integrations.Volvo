using Karmak.Integrations.Volvo.Oasis.Models;
using Karmak.Integrations.Volvo.Oasis.Models.Xml;

namespace Karmak.Integrations.Volvo.Oasis.Services
{
    public interface IOasisClient
    {
        Task<OasisResponseRest> SendAsync(string oasisUri, OasisRequest request, string karmakAccountNumber = null);
    }
}
