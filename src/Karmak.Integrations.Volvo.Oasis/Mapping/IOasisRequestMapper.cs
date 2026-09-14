using Karmak.Integrations.Volvo.Oasis.Models;
using Karmak.Integrations.Volvo.Oasis.Models.Xml;

namespace Karmak.Integrations.Volvo.Oasis.Mapping
{
    public interface IOasisRequestMapper
    {
        OasisRequest Map(OasisRequestRest source);
    }
}
