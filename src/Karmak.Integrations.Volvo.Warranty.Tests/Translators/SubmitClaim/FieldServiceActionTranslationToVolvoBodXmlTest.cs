using System.Xml.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim
{
    public class FieldServiceActionTranslationToVolvoBodXmlTest
    {
        private static readonly SubmitClaimTranslator _submitClaimTranslator = new SubmitClaimTranslator(SubmitClaimJobTranslatorFactory.Create(), "Test");

        [Fact]
        public void WhenTranslatingAndSerializing_It_IncludesCampaignOptionCode()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.FieldServiceAction;
            claim.InAppeal = false;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    CampaignOptionCode = "010101"
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                <ServiceCampaign xmlns=""{TestConstants.STAR_NAMESPACE}"">
                    <CampaignOptionCode>010101</CampaignOptionCode>
                </ServiceCampaign>"));
        }

        [Fact]
        public void WhenMissingCampaignOptionsCode_It_IsExcluded()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.FieldServiceAction;
            claim.InAppeal = false;
            claim.RepairOrder.Jobs = new[] {
                new Job()
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertNotContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                <ServiceCampaign xmlns=""{TestConstants.STAR_NAMESPACE}"" />"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_IncludesRelatedDamageIndicator()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.FieldServiceAction;
            claim.InAppeal = false;
            claim.IsRelatedDamageIncluded = true;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "WarrantyClaimExtended", XElement.Parse($@"
                <RelatedDamageIndicator xmlns=""{TestConstants.OWS_NAMESPACE}"">true</RelatedDamageIndicator>"));
        }

        [Fact]
        public void WhenRelatedDamageIndicatorIsFalse_It_ExcludesIt()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.FieldServiceAction;
            claim.InAppeal = false;
            claim.IsRelatedDamageIncluded = false;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertNotContainsIn(TestConstants.OWS_NAMESPACE + "WarrantyClaimExtended", XElement.Parse($@"
                <RelatedDamageIndicator xmlns=""{TestConstants.OWS_NAMESPACE}"">false</RelatedDamageIndicator>"));
        }
    }
}
