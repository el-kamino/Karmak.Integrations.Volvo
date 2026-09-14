using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.Fusion.React;

public interface IDealerInfoExtractor
{
    DealerInfo Extract();
    bool TryExtractElkContextToDealerInfo(ElkContext context, out DealerInfo dealerInfo);
}
