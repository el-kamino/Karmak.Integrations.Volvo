using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim
{
    public class SubmitClaimJobTranslatorArguments : ITranslatorArguments<IClaim>
    {
        public IClaim Source { get; set; }
        public CurrencyCode DealerRegionCurrency { get; set; }
    }
}
