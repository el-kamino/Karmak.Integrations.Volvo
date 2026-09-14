using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.Fusion.React;

public class DealerInfoExtractor : IDealerInfoExtractor
{
    public DealerInfo Extract()
    {
        if (!ImplicitElkContext.TryGetCurrent(out var context))
        {
            throw new InvalidOperationException($"must supply a valid elk Context");
        }

        return new DealerInfo()
        {
            AccountId = context.Identity.Account,
            BranchId = context.ApplicationContext.Branch ?? throw new InvalidOperationException($"elk context must have a valid Branch"),
            InstanceId = context.ApplicationContext.Instance ?? throw new InvalidOperationException($"elk context must have a valid Instance")
        };
    }

    public bool TryExtractElkContextToDealerInfo(ElkContext context, out DealerInfo dealerInfo)
    {
        dealerInfo = null;

        if (context.ApplicationContext?.Instance == null || context.ApplicationContext?.Branch == null)
        {
            return false;
        }

        dealerInfo = new DealerInfo
        {
            AccountId = context.Identity.Account,
            BranchId = context.ApplicationContext.Branch.Value,
            InstanceId = context.ApplicationContext.Instance.Value
        };

        return true;
    }
}