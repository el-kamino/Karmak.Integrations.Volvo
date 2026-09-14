using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;

namespace Karmak.Integrations.Volvo.Warranty.Translators
{
    public class GetClaimReconciliationTranslatorArguments : ITranslatorArguments<ReconciliationFetchRequest>
    {
        public ReconciliationFetchRequest Source { get; set; }
        public VolvoSettings Settings { get; set; }
    }
}
