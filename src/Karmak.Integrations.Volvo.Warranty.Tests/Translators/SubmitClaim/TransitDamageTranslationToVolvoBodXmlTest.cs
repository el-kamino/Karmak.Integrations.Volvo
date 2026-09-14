using System.Xml.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim
{
    public class TransitDamageTranslationToVolvoBodXmlTest
    {
        private static readonly SubmitClaimTranslator _submitClaimTranslator = new SubmitClaimTranslator(SubmitClaimJobTranslatorFactory.Create(), "Test");

        [Fact]
        public void WhenTranslatingAndSerializing_It_IncludesTransportation()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.TransitDamage;
            claim.InAppeal = false;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    Transportation = new Transportation {
                        CarrierName = "Ted Nugent",
                        InvoiceIdentifier = "PO12345",
                        Damage = new Damage {
                            Area = "12",
                            Type = "34",
                            Severity = "5"
                        },
                        ArrivalDate = new DateTime(2019, 01, 02, 7, 30, 21, 45, DateTimeKind.Utc)
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <Transportation xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <TransportationCarrierName>Ted Nugent</TransportationCarrierName>
                        <ReceiptNumberString>PO12345</ReceiptNumberString>
                        <VehicleArrivalDate>2019-01-02</VehicleArrivalDate>
                        <Damage>
                            <DamageCode>12345</DamageCode>
                        </Damage>
                    </Transportation>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_DoesNotIncludeNullTransportationArrivalDate()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.TransitDamage;
            claim.InAppeal = false;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    Transportation = new Transportation {
                        CarrierName = "Ted Nugent",
                        InvoiceIdentifier = "PO12345",
                        Damage = new Damage {
                            Area = "12",
                            Type = "34",
                            Severity = "5"
                        },
                        ArrivalDate = null
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <Transportation xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <TransportationCarrierName>Ted Nugent</TransportationCarrierName>
                        <ReceiptNumberString>PO12345</ReceiptNumberString>
                        <Damage>
                            <DamageCode>12345</DamageCode>
                        </Damage>
                    </Transportation>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_ExcludesNullDamage()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.TransitDamage;
            claim.InAppeal = false;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    Transportation = new Transportation {
                        CarrierName = "Ted Nugent",
                        InvoiceIdentifier = "PO12345",
                        Damage = null,
                        ArrivalDate = new DateTime(2019, 01, 02, 7, 30, 21, 45, DateTimeKind.Utc)
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <Transportation xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <TransportationCarrierName>Ted Nugent</TransportationCarrierName>
                        <ReceiptNumberString>PO12345</ReceiptNumberString>
                        <VehicleArrivalDate>2019-01-02</VehicleArrivalDate>
                    </Transportation>"));
        }

        [Fact]
        public void WhenMissingTransportation_It_IsExcluded()
        {
            var claim = FakeClaim.Generate();
            claim.Type = ClaimTypes.TransitDamage;
            claim.InAppeal = false;
            claim.RepairOrder.Jobs = new[] {
                new Job()
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertNotContainsIn(TestConstants.STAR_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <Transportation xmlns=""{TestConstants.STAR_NAMESPACE}"" />"));
        }
    }
}
