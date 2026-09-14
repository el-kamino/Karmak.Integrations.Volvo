using System.Xml.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim
{
    public class ExtendedServiceContractTranslationToVolvoBodXmlTest
    {
        private static readonly SubmitClaimTranslator _submitClaimTranslator = new SubmitClaimTranslator(SubmitClaimJobTranslatorFactory.Create(), "Test");

        [Fact]
        public void WhenIsExtendedServiceContractFranchise_It_IncludesRepairOrderExtendedServiceContractFranchise()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.ExtendedServiceContracts;
            claim.InAppeal = false;
            claim.RepairOrder.IsExtendedServiceContractFranchise = true;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <ESCFranchiseIndicator xmlns=""{TestConstants.STAR_NAMESPACE}"">true</ESCFranchiseIndicator>"));
        }

        [Fact]
        public void WhenIsNotExtendedServiceContractFranchise_It_ExcludesRepairOrderExtendedServiceContractFranchise()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.ExtendedServiceContracts;
            claim.InAppeal = false;
            claim.RepairOrder.IsExtendedServiceContractFranchise = false;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertNotContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <ESCFranchiseIndicator xmlns=""{TestConstants.STAR_NAMESPACE}"">false</ESCFranchiseIndicator>"));
        }
    }
}
