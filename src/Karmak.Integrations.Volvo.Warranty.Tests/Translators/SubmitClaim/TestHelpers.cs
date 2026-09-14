using System.Xml.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml;
using Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim.Data;
using Karmak.Integrations.Volvo.Warranty.Translators;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim
{
    public static class TestHelpers
    {
        public static XDocument TranslateAndSerialize(this ITranslatable<SubmitClaimTranslatorArguments, ProcessRepairOrderType> translator, IEnumerable<Claim> claims) =>
            XDocumentExtended.Serialize(
                translator.Translate(new SubmitClaimTranslatorArguments()
                {
                    Source = claims,
                    Settings = FakeSettings.Instance
                }));
    }
}
