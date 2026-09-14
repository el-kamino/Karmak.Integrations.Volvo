using System.Xml.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim
{
    public class FleetTranslationToVolvoBodXmlTest
    {

        private static readonly SubmitClaimTranslator _submitClaimTranslator = new SubmitClaimTranslator(SubmitClaimJobTranslatorFactory.Create(), "Test");

        [Fact]
        public void WhenTranslatingAndSerializingPartExpenses_It_IncludesFleetDiscount()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.Fleet;
            claim.InAppeal = false;
            claim.FleetAccount.PartsDiscountPercentage = 32.23m;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    PartExpenses = new[] {
                        new PartExpense {
                            Identifier = "1234",
                            Prefix = "abc",
                            Number = "1234",
                            Suffix = "efg",
                            Quantity = new Quantity {
                                Type = UnitOfMeasureType.Count,
                                Value = 1
                            },
                            UnitPrice = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 15.00m
                            }
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <ServicePartsExtended xmlns=""{TestConstants.OWS_NAMESPACE}"">
                        <ItemQuantity xmlns=""{TestConstants.STAR_NAMESPACE}"">1</ItemQuantity>
                        <Pricing xmlns=""{TestConstants.STAR_NAMESPACE}"">
                            <Price>
                                <ChargeAmount currencyID=""USD"">15.00</ChargeAmount>
                                <PriceDescription>PART EXTENDED AMOUNT</PriceDescription>
                            </Price>
                        </Pricing>
                        <ItemIdentificationGroup xmlns=""{TestConstants.STAR_NAMESPACE}"">
                            <ItemIdentification>
                                <ItemID>abc!1234!efg</ItemID>
                            </ItemIdentification>
                        </ItemIdentificationGroup>
                        <FleetDiscountPercent>32.23</FleetDiscountPercent>
                    </ServicePartsExtended>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializingPartExpensesWithoutFleetAccount_It_ExcludesFleetDiscount()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.Fleet;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    PartExpenses = new[] {
                        new PartExpense {Identifier = "1234"}
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertNotContainsIn(TestConstants.STAR_NAMESPACE + "ServicePartsExtended", XElement.Parse($@"
                    <FleetDiscountPercent xmlns=""{TestConstants.OWS_NAMESPACE}"">{claim.FleetAccount?.PartsDiscountPercentage}</FleetDiscountPercent>"));
        }
    }
}
