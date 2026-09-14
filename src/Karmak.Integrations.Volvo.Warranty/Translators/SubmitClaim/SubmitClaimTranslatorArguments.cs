using System.Collections.Generic;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim
{
    public class SubmitClaimTranslatorArguments : ITranslatorArguments<IEnumerable<IClaim>>
    {
        public IEnumerable<IClaim> Source { get; set; }
        public VolvoSettings Settings { get; set; }
    }
}
