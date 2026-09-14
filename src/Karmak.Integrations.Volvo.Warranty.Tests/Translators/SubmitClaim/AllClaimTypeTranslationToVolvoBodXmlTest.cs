using System.Xml.Linq;
using AutoBogus;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing;
using Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim.Data;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using PreviousPartClaim = Karmak.Integrations.Volvo.Warranty.Contracts.PreviousPartClaim;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim
{
    public class AllClaimTypeTranslationToVolvoBodXmlTest
    {
        private static readonly SubmitClaimTranslator _submitClaimTranslator = new SubmitClaimTranslator(SubmitClaimJobTranslatorFactory.Create(), "Test");

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingWithNullSettings_It_Throws(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;

            var args = new SubmitClaimTranslatorArguments()
            {
                Source = new List<Claim>() { claim },
                Settings = null
            };

            Assert.Throws<ArgumentNullException>(() => _submitClaimTranslator.Translate(args));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingWithNullDealerServiceProviderSettings_It_Throws(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;

            Assert.Throws<ArgumentNullException>(() =>
                _submitClaimTranslator.Translate(
                    new SubmitClaimTranslatorArguments()
                    {
                        Source = new List<Claim>() { claim },
                        Settings = new VolvoSettings
                        {
                            DealerServiceProviderSettings = null,
                            InterfaceOptions = AutoFaker.Generate<InterfaceOptions>(),
                            RegionSettings = AutoFaker.Generate<RegionSettings>()
                        }
                    })
                );
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingWithNullInterfaceOptions_It_Throws(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;

            Assert.Throws<ArgumentNullException>(() =>
                _submitClaimTranslator.Translate(
                    new SubmitClaimTranslatorArguments()
                    {
                        Source = new List<Claim>() { claim },
                        Settings = new VolvoSettings
                        {
                            DealerServiceProviderSettings = AutoFaker.Generate<DealerServiceProviderSettings>(),
                            InterfaceOptions = null,
                            RegionSettings = AutoFaker.Generate<RegionSettings>()
                        }
                    })
                );
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingWithNullRegionSettings_It_Throws(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;

            Assert.Throws<ArgumentNullException>(() =>
                _submitClaimTranslator.Translate(
                    new SubmitClaimTranslatorArguments()
                    {
                        Source = new List<Claim>() { claim },
                        Settings = new VolvoSettings
                        {
                            DealerServiceProviderSettings = AutoFaker.Generate<DealerServiceProviderSettings>(),
                            InterfaceOptions = AutoFaker.Generate<InterfaceOptions>(),
                            RegionSettings = null
                        }
                    })
                );
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingEmptyClaim_It_IncludesEmptyRepairOrder(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder = null;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertNotContainsName(
                TestConstants.STAR_NAMESPACE + "ProcessRepairOrderDataArea",
                TestConstants.STAR_NAMESPACE + "RepairOrder");
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenSerializing_It_IncludesRootAttributes(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsWithAttributes(XElement.Parse($@"
                    <ProcessRepairOrder
                        xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                        xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                        releaseID=""5.2.4""
                        systemEnvironmentCode=""Test""
                        languageCode=""en-US""
                        xmlns=""http://www.starstandard.org/STAR/5"" />"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesSender(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "ApplicationArea", XElement.Parse($@"
                    <Sender xmlns=""{TestConstants.STAR_NAMESPACE}"">
                      <ComponentID>Fusion</ComponentID>
                      <TaskID>RepairOrder</TaskID>
                      <ConfirmationCode>Never</ConfirmationCode>
                      <CreatorNameCode>Karmak</CreatorNameCode>
                      <SenderNameCode>KM</SenderNameCode>
                      <DealerNumberID>K1234!</DealerNumberID>
                      <DealerCountryCode>US</DealerCountryCode>
                      <LanguageCode>en-US</LanguageCode>
                      <SystemVersion>1.0.0</SystemVersion>
                    </Sender>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesDestination(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "ApplicationArea", XElement.Parse($@"
                     <Destination xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <DestinationNameCode>FM</DestinationNameCode>
                        <DestinationSoftwareCode>OWS</DestinationSoftwareCode>
                        <DestinationSoftware>1.0</DestinationSoftware>
                        <ServiceMessageID>ONE Warranty Solution</ServiceMessageID>
                      </Destination>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializingWithNullApprovalIdentifiers_It_IncludesRepairOrderSecondaryIdentifier(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.RepairOrder.ApprovalIdentifiers = null;
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder.SecondaryIdentifier = "111111";
            claim.RepairOrder.Identifier = "900000";

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <DocumentIdentificationGroup xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <DocumentIdentification>
                            <DocumentID>111111</DocumentID>
                        </DocumentIdentification>
                    </DocumentIdentificationGroup>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializingWithEmptyApprovalIdentifiers_It_IncludesRepairOrderSecondaryIdentifier(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.RepairOrder.ApprovalIdentifiers = AutoFaker.Generate<string>(0);
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder.SecondaryIdentifier = "111111";
            claim.RepairOrder.Identifier = "900000";

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <DocumentIdentificationGroup xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <DocumentIdentification>
                            <DocumentID>111111</DocumentID>
                        </DocumentIdentification>
                    </DocumentIdentificationGroup>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializingWithApprovalCodes_It_IncludesRepairOrderIdentifier(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.RepairOrder.ApprovalIdentifiers = AutoFaker.Generate<string>(3);
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder.SecondaryIdentifier = "100000";
            claim.RepairOrder.Identifier = "999999";

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <DocumentIdentificationGroup xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <DocumentIdentification>
                            <DocumentID>999999</DocumentID>
                        </DocumentIdentification>
                    </DocumentIdentificationGroup>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesDealerParty(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <DealerParty xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <PartyID>K1234!</PartyID>
                        <LocationID>US</LocationID>
                    </DealerParty>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesRepairOrderOpenedDate(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder.OpenedDate = new DateTime(2019, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <RepairOrderOpenedDate xmlns=""{TestConstants.STAR_NAMESPACE}"">2019-01-01</RepairOrderOpenedDate>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesRepairOrderServiceAdvisor(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder.Advisor.Identifier = "010203";

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <ServiceAdvisorParty xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <PartyID>010203</PartyID>
                    </ServiceAdvisorParty>"));
        }

        [Theory]
        [InlineData(ClaimTypes.VehicleCoverages, false, ClaimTypes.VehicleCoverages)]
        [InlineData(ClaimTypes.VehicleCoverages, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.PreDeliveryInspection, false, ClaimTypes.PreDeliveryInspection)]
        [InlineData(ClaimTypes.PreDeliveryInspection, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.Policy, false, ClaimTypes.Policy)]
        [InlineData(ClaimTypes.Policy, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.MisBuilt, false, ClaimTypes.MisBuilt)]
        [InlineData(ClaimTypes.MisBuilt, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.ServiceParts, false, ClaimTypes.ServiceParts)]
        [InlineData(ClaimTypes.ServiceParts, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.OverTheCounter, false, ClaimTypes.OverTheCounter)]
        [InlineData(ClaimTypes.OverTheCounter, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.Accessories, false, ClaimTypes.Accessories)]
        [InlineData(ClaimTypes.Accessories, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.FieldServiceAction, false, ClaimTypes.FieldServiceAction)]
        [InlineData(ClaimTypes.FieldServiceAction, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.FreeInspection, false, ClaimTypes.FreeInspection)]
        [InlineData(ClaimTypes.FreeInspection, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.TransitDamage, false, ClaimTypes.TransitDamage)]
        [InlineData(ClaimTypes.TransitDamage, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.Fleet, false, ClaimTypes.Fleet)]
        [InlineData(ClaimTypes.Fleet, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.ExtendedServiceContracts, false, ClaimTypes.ExtendedServiceContracts)]
        [InlineData(ClaimTypes.ExtendedServiceContracts, true, ClaimTypes.Appeal)]
        [InlineData(ClaimTypes.RetailCoreReturn, false, ClaimTypes.RetailCoreReturn)]
        [InlineData(ClaimTypes.RetailCoreReturn, true, ClaimTypes.Appeal)]
        public void WhenTranslatingAndSerializing_It_IncludesJobTypeString(string claimType, bool inAppeal, string expectedClaimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = inAppeal;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <JobTypeString xmlns=""{TestConstants.STAR_NAMESPACE}"">{expectedClaimType}</JobTypeString>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesJobNumberString(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder.Jobs.First().Identifier = "A";

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <JobNumberString xmlns=""{TestConstants.STAR_NAMESPACE}"">A</JobNumberString>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesPreAuthorizationIdentifier(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.PreAuthorizationIdentifier = "123abc";

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <PreDefinedRepairCode xmlns=""{TestConstants.OWS_NAMESPACE}"">123abc</PreDefinedRepairCode>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesHoldAtPreValidationIndicator(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.ShouldHoldAtPreValidation = true;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <HoldAtPreValidationIndicator xmlns=""{TestConstants.OWS_NAMESPACE}"">true</HoldAtPreValidationIndicator>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesJobCompletionDate(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder.CompletedDate = new DateTime(2019, 01, 02, 0, 0, 0, 0, DateTimeKind.Utc);

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <JobCompletionDate xmlns=""{TestConstants.STAR_NAMESPACE}"">2019-01-02</JobCompletionDate>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesPartExpenses(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    PartExpenses = new [] {
                        new PartExpense {
                            Identifier = "1234",
                            Prefix = "",
                            Number = "1234",
                            Suffix = "",
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
                                <ItemID>!1234!</ItemID>
                            </ItemIdentification>
                        </ItemIdentificationGroup>
                    </ServicePartsExtended>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesPrefixAndSuffixForThePartItemId(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    PartExpenses = new [] {
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
                    </ServicePartsExtended>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesLaborExpenses(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    LaborExpenses = new[] {
                        new LaborExpense {
                            Identifier = "5678",
                            ThirdPartyInvoiceIdentifier = "B1234F890976529",
                            Technician = new User {
                                Identifier = "010203",
                                Username = "tnugent"
                            },
                            Quantity = new Quantity {
                                Type = UnitOfMeasureType.Hours,
                                Value = 3.3m
                            },
                            UnitPrice = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 86.26m
                            }
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <ServiceLaborExtended xmlns=""{TestConstants.OWS_NAMESPACE}"">
                        <LaborOperationID xmlns=""{TestConstants.STAR_NAMESPACE}"">5678</LaborOperationID>
                        <LaborActualHoursNumeric xmlns=""{TestConstants.STAR_NAMESPACE}"">3.3</LaborActualHoursNumeric>
                        <ServiceTechnicianParty xmlns=""{TestConstants.STAR_NAMESPACE}"">
                            <PartyID>010203</PartyID>
                        </ServiceTechnicianParty>
                        <Sublet xmlns=""{TestConstants.STAR_NAMESPACE}"">
                            <Pricing>
                                <Price />
                            </Pricing>
                          <SubletCode>Default</SubletCode>
                          <SubletWorkDescription>Default</SubletWorkDescription>
                          <SubletInvoiceNumberString>B1234F890976529</SubletInvoiceNumberString>
                        </Sublet>
                    </ServiceLaborExtended>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesTotalLaborExpenseAmount(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    LaborExpenses = new[] {
                        new LaborExpense {
                            Identifier = "5678",
                            Quantity = new Quantity {
                                Type = UnitOfMeasureType.Hours,
                                Value = 3.3m
                            },
                            UnitPrice = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 86.26m
                            }
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <Pricing xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <Price>
                            <ChargeAmount currencyID=""USD"">284.66</ChargeAmount>
                            <PriceDescription>Labor Total</PriceDescription>
                        </Price>
                    </Pricing>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenMissingTechnicianUsername_It_ExcludesItFromLaborExpenses(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    LaborExpenses = new[] {
                        new LaborExpense {
                            Identifier = "1234",
                            Technician = null
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertNotContainsIn(TestConstants.OWS_NAMESPACE + "ServiceLaborExtended", XElement.Parse($@"
                    <ServiceTechnicianParty xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <PartyID>{claim.RepairOrder.Jobs.First().LaborExpenses.First().Technician?.Username}</PartyID>
                    </ServiceTechnicianParty>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesJobChargeAmountInParts(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    PartExpenses = new[] {
                        new PartExpense {
                            Identifier = "1234",
                            Quantity = new Quantity {
                                Type = UnitOfMeasureType.Count,
                                Value = 50.00m
                            },
                            CorePrice = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 100m
                            }
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "ServicePartsExtended", XElement.Parse($@"
                    <Pricing xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <Price>
                            <ChargeAmount currencyID=""USD"">100</ChargeAmount>
                            <PriceDescription>CORE AMOUNT</PriceDescription>
                        </Price>
                    </Pricing>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesTotalLaborPricingInJob(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    LaborExpenses = new[] {
                        new LaborExpense {
                            Identifier = "1234",
                            Quantity = new Quantity {
                                Type = UnitOfMeasureType.Hours,
                                Value = 1m
                            },
                            UnitPrice = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 100m
                            }
                        },
                        new LaborExpense {
                            Identifier = "5678",
                            Quantity = new Quantity {
                                Type = UnitOfMeasureType.Hours,
                                Value = 2m
                            },
                            UnitPrice = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 50m
                            }
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <Pricing xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <Price>
                            <ChargeAmount currencyID=""USD"">200</ChargeAmount>
                            <PriceDescription>Labor Total</PriceDescription>
                        </Price>
                    </Pricing>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_HandlesNullUnitPriceInLaborExpenses(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    LaborExpenses = new[] {
                        new LaborExpense {
                            Identifier = "1234"
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <Pricing xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <Price>
                            <ChargeAmount currencyID=""USD"">0</ChargeAmount>
                            <PriceDescription>Labor Total</PriceDescription>
                        </Price>
                    </Pricing>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesPricingInLabor(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    LaborExpenses = new[] {
                        new LaborExpense {
                            Identifier = "1234"
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "ServiceLaborExtended", XElement.Parse($@"
                    <Pricing xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <Price />
                    </Pricing>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesMiscellaneousExpenses(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    MiscellaneousExpenses = new[] {
                        new MiscellaneousExpense {
                            Identifier = "1234",
                            UnitPrice = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 1000.5m
                            },
                            Quantity = new Quantity {
                                Type = UnitOfMeasureType.Days,
                                Value = 2m
                            },
                            SecondQuantity = new Quantity {
                                Type = UnitOfMeasureType.Hours,
                                Value = 0m
                            }
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <ServiceComponentsExtended xmlns=""{TestConstants.OWS_NAMESPACE}"">
                        <ComponentTypeCode xmlns=""{TestConstants.STAR_NAMESPACE}"">Miscellaneous</ComponentTypeCode>
                        <ServiceCode xmlns=""{TestConstants.STAR_NAMESPACE}"">1234</ServiceCode>
                        <Pricing xmlns=""{TestConstants.STAR_NAMESPACE}"">
                            <Price>
                                <ChargeAmount currencyID=""USD"">2001.0</ChargeAmount>
                            </Price>
                        </Pricing>
                        <ExpenseDaysNumeric>2</ExpenseDaysNumeric>
                    </ServiceComponentsExtended>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesMiscellaneousExpenses_UsesHourlyExpenseWhenNoDayQuantity(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    MiscellaneousExpenses = new[] {
                        new MiscellaneousExpense {
                            Identifier = "1234",
                            UnitPrice = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 1000.5m
                            },
                            Quantity = new Quantity {
                                Type = UnitOfMeasureType.Days,
                                Value = 0m
                            },
                            SecondQuantity = new Quantity {
                                Type = UnitOfMeasureType.Hours,
                                Value = 2m
                            }
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <ServiceComponentsExtended xmlns=""{TestConstants.OWS_NAMESPACE}"">
                        <ComponentTypeCode xmlns=""{TestConstants.STAR_NAMESPACE}"">Miscellaneous</ComponentTypeCode>
                        <ServiceCode xmlns=""{TestConstants.STAR_NAMESPACE}"">1234</ServiceCode>
                        <Pricing xmlns=""{TestConstants.STAR_NAMESPACE}"">
                            <Price>
                                <ChargeAmount currencyID=""USD"">2001.0</ChargeAmount>
                            </Price>
                        </Pricing>
                        <ExpenseHoursNumeric xmlns=""{TestConstants.STAR_NAMESPACE}"">2</ExpenseHoursNumeric>
                    </ServiceComponentsExtended>"));
            xml.AssertNotContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <ServiceComponentsExtended xmlns=""{TestConstants.OWS_NAMESPACE}"">
                        <ExpenseDaysNumeric>0</ExpenseDaysNumeric>
                    </ServiceComponentsExtended>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesDriverParty(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.Driver = new Driver
            {
                Identifier = "1234",
                FullName = "John A Doe"
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrder", XElement.Parse($@"
                    <PrimaryDriver xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <DriverParty>
                            <PartyID>1234</PartyID>
                            <SpecifiedPerson>
                                <GivenName>John A Doe</GivenName>
                            </SpecifiedPerson>
                        </DriverParty>
                    </PrimaryDriver>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenMissingDriver_It_ExcludesDriverParty(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.Driver = null;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertNotContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrder", XElement.Parse($@"
                    <PrimaryDriver xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <DriverParty>
                            <SpecifiedPerson />
                        </DriverParty>
                    </PrimaryDriver>"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesAnEmptyServiceTechnicianPartyOnTheJob(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <ServiceTechnicianParty xmlns=""{TestConstants.STAR_NAMESPACE}"" />"));
        }

        [Theory]
        [ClassData(typeof(AllClaimTypes))]
        public void WhenTranslatingAndSerializing_It_IncludesDiagnosticCodes(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    Diagnostics = new Karmak.Integrations.Volvo.Warranty.Contracts.Diagnostics {
                        IsCheckEngineLightOn = true,
                        DiagnosticTroubleCodes = new[] {"01"},
                        PolicyRequiredMeasurementOrResults = new[] {"02", "03"},
                        BatteryCodes = new[] {"04"},
                        BodyCodes = new[] {"05"},
                        ChassisCodes = new[] {"06"},
                        UndefinedDiagnosticCodes = new[] {"07"},
                        KeyOnEngineOffCodes = new[] {"08"},
                        KeyOnEngineColdCodes = new[] {"09"},
                        KeyOnEngineRunningCodes = new[] {"10"},
                        ReplacementTireDepartmentOfTransportationCodes = new[] {"11"},
                        ReplacedTireDepartmentOfTransportationCodes = new[] {"12", "13"}
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <DiagnosticsExtended xmlns=""{TestConstants.OWS_NAMESPACE}"">
                        <DiagnosticCodes xmlns=""{TestConstants.STAR_NAMESPACE}"">01</DiagnosticCodes>
                        <EngineLightIndicator xmlns=""{TestConstants.STAR_NAMESPACE}"">true</EngineLightIndicator>
                        <BatteryCodes>04</BatteryCodes>
                        <BodyCodes>05</BodyCodes>
                        <ChassisCodes>06</ChassisCodes>
                        <UndefinedDiagnosticCodes>07</UndefinedDiagnosticCodes>
                        <ReqMeasurementOrResults>02</ReqMeasurementOrResults>
                        <ReqMeasurementOrResults>03</ReqMeasurementOrResults>
                        <KOEOCodes>08</KOEOCodes>
                        <KOECCodes>09</KOECCodes>
                        <KOERCodes>10</KOERCodes>
                        <ReplacedTireDOTCode>12</ReplacedTireDOTCode>
                        <ReplacedTireDOTCode>13</ReplacedTireDOTCode>
                        <ReplacementTireDOTCode>11</ReplacementTireDOTCode>
                    </DiagnosticsExtended>"));
        }

        [Theory]
        [InlineData(ClaimTypes.VehicleCoverages, false)]
        [InlineData(ClaimTypes.VehicleCoverages, true)]
        [InlineData(ClaimTypes.PreDeliveryInspection, false)]
        [InlineData(ClaimTypes.PreDeliveryInspection, true)]
        [InlineData(ClaimTypes.Policy, false)]
        [InlineData(ClaimTypes.Policy, true)]
        [InlineData(ClaimTypes.MisBuilt, false)]
        [InlineData(ClaimTypes.MisBuilt, true)]
        [InlineData(ClaimTypes.ServiceParts, false)]
        [InlineData(ClaimTypes.ServiceParts, true)]
        [InlineData(ClaimTypes.OverTheCounter, false)]
        [InlineData(ClaimTypes.OverTheCounter, true)]
        [InlineData(ClaimTypes.Accessories, false)]
        [InlineData(ClaimTypes.Accessories, true)]
        [InlineData(ClaimTypes.FieldServiceAction, false)]
        [InlineData(ClaimTypes.FieldServiceAction, true)]
        [InlineData(ClaimTypes.FreeInspection, false)]
        [InlineData(ClaimTypes.FreeInspection, true)]
        [InlineData(ClaimTypes.TransitDamage, false)]
        [InlineData(ClaimTypes.TransitDamage, true)]
        [InlineData(ClaimTypes.Fleet, false)]
        [InlineData(ClaimTypes.Fleet, true)]
        [InlineData(ClaimTypes.ExtendedServiceContracts, false)]
        [InlineData(ClaimTypes.ExtendedServiceContracts, true)]
        public void WhenTranslatingAndSerializing_It_IncludesTheVehicleLineItem(string claimType, bool inAppeal)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = inAppeal;
            claim.FleetAccount = null;
            claim.Unit = new Unit
            {
                Vehicle = new Vehicle
                {
                    Identifier = "1FTNS14L69DA53382",
                    SpecialUseIdentifier = "Police Car"
                },
                MeterReadings = new[] {
                    new MeterReading {
                        Type = MeterReadingType.EngineHours,
                        ReadingIn = new Measurement {
                            UnitOfMeasure = UnitOfMeasureType.Hours,
                            Value = 10123m
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.STAR_NAMESPACE + "RepairOrderHeader", XElement.Parse($@"
                    <RepairOrderVehicleLineItemExtended xmlns=""{TestConstants.OWS_NAMESPACE}"">
                        <Vehicle xmlns=""{TestConstants.STAR_NAMESPACE}"">
                            <VehicleNote>Police Car</VehicleNote>
                                <Engine>
                                    <TotalEngineHoursNumeric>10123</TotalEngineHoursNumeric>
                                </Engine>
                            <VehicleID>1FTNS14L69DA53382</VehicleID>
                        </Vehicle>
                     </RepairOrderVehicleLineItemExtended>"));
        }

        [Theory]
        [InlineData(ClaimTypes.Accessories)]
        [InlineData(ClaimTypes.OverTheCounter)]
        [InlineData(ClaimTypes.ServiceParts)]
        public void WhenTranslatingAndSerializing_It_IncludesSparePartDate(string claimType)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = false;
            claim.FleetAccount = null;
            claim.RepairOrder.Jobs = new[] {
                new Job {
                    PreviousPartClaim = new PreviousPartClaim {
                        InvoiceDate = new DateTime(2019, 01, 03, 9, 45, 21, 45, DateTimeKind.Utc)
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "WarrantyClaimExtended", XElement.Parse($@"
                    <SparePartDate xmlns=""{TestConstants.STAR_NAMESPACE}"">2019-01-03</SparePartDate>"));
        }

        [Theory]
        [InlineData(ClaimTypes.VehicleCoverages, false)]
        [InlineData(ClaimTypes.VehicleCoverages, true)]
        [InlineData(ClaimTypes.PreDeliveryInspection, false)]
        [InlineData(ClaimTypes.PreDeliveryInspection, true)]
        [InlineData(ClaimTypes.Policy, false)]
        [InlineData(ClaimTypes.Policy, true)]
        [InlineData(ClaimTypes.MisBuilt, false)]
        [InlineData(ClaimTypes.MisBuilt, true)]
        [InlineData(ClaimTypes.ServiceParts, false)]
        [InlineData(ClaimTypes.ServiceParts, true)]
        [InlineData(ClaimTypes.OverTheCounter, false)]
        [InlineData(ClaimTypes.OverTheCounter, true)]
        [InlineData(ClaimTypes.Accessories, false)]
        [InlineData(ClaimTypes.Accessories, true)]
        [InlineData(ClaimTypes.FieldServiceAction, false)]
        [InlineData(ClaimTypes.FieldServiceAction, true)]
        [InlineData(ClaimTypes.FreeInspection, false)]
        [InlineData(ClaimTypes.FreeInspection, true)]
        [InlineData(ClaimTypes.TransitDamage, false)]
        [InlineData(ClaimTypes.TransitDamage, true)]
        [InlineData(ClaimTypes.Fleet, false)]
        [InlineData(ClaimTypes.Fleet, true)]
        [InlineData(ClaimTypes.ExtendedServiceContracts, false)]
        [InlineData(ClaimTypes.ExtendedServiceContracts, true)]
        public void WhenTranslatingAndSerializing_It_IncludesOutEngOperatingHoursNumeric(string claimType, bool inAppeal)
        {
            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = inAppeal;
            claim.Unit = new Unit
            {
                MeterReadings = new[] {
                    new MeterReading {
                        Type = MeterReadingType.EngineHours,
                        ReadingIn = new Measurement {
                            UnitOfMeasure = UnitOfMeasureType.Hours,
                            Value = 115m
                        },
                        ReadingOut = new Measurement {
                            UnitOfMeasure = UnitOfMeasureType.Hours,
                            Value = 120m
                        }
                    }
                }
            };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <OutEngOperatingHoursNumeric xmlns=""{TestConstants.OWS_NAMESPACE}"">120</OutEngOperatingHoursNumeric>"));
        }

        [Theory]
        [InlineData(ClaimTypes.VehicleCoverages, false)]
        [InlineData(ClaimTypes.VehicleCoverages, true)]
        [InlineData(ClaimTypes.Policy, false)]
        [InlineData(ClaimTypes.Policy, true)]
        [InlineData(ClaimTypes.MisBuilt, false)]
        [InlineData(ClaimTypes.MisBuilt, true)]
        [InlineData(ClaimTypes.ServiceParts, false)]
        [InlineData(ClaimTypes.ServiceParts, true)]
        [InlineData(ClaimTypes.OverTheCounter, false)]
        [InlineData(ClaimTypes.OverTheCounter, true)]
        [InlineData(ClaimTypes.Accessories, false)]
        [InlineData(ClaimTypes.Accessories, true)]
        [InlineData(ClaimTypes.FreeInspection, false)]
        [InlineData(ClaimTypes.FreeInspection, true)]
        [InlineData(ClaimTypes.ExtendedServiceContracts, false)]
        [InlineData(ClaimTypes.ExtendedServiceContracts, true)]
        [InlineData(ClaimTypes.FieldServiceAction, false)]
        public void WhenTranslatingAndSerializing_It_IncludesCustomerComplaintAndNotes(string claimType, bool inAppeal)
        {
            var Job = new Job
            {
                ComplaintCode = "A1",
                CustomerNotes = "Something is seriously wrong.",
                TechnicianNotes = "Yes, something is seriously wrong.",
            };

            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = inAppeal;
            claim.RepairOrder.Jobs = new[] { Job };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <CodesAndCommentsExpanded xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <ComplaintCode>{Job.ComplaintCode}</ComplaintCode>
                        <ComplaintDescription>{Job.CustomerNotes}</ComplaintDescription>
                        <TechnicianNotes>{Job.TechnicianNotes}</TechnicianNotes>
                    </CodesAndCommentsExpanded>"));
        }

        [Theory]
        [InlineData(ClaimTypes.PreDeliveryInspection, false)]
        [InlineData(ClaimTypes.TransitDamage, false)]
        [InlineData(ClaimTypes.Fleet, false)]
        public void WhenTranslatingAndSerializing_It_IncludesNotes(string claimType, bool inAppeal)
        {
            var Job = new Job
            {
                ComplaintCode = "A1",
                CustomerNotes = "Something is seriously wrong.",
                TechnicianNotes = "Yes, something is seriously wrong.",
            };

            var claim = FakeClaim.Generate();
            claim.Type = claimType;
            claim.InAppeal = inAppeal;
            claim.RepairOrder.Jobs = new[] { Job };

            var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

            xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "JobExtended", XElement.Parse($@"
                    <CodesAndCommentsExpanded xmlns=""{TestConstants.STAR_NAMESPACE}"">
                        <ComplaintDescription>{Job.CustomerNotes}</ComplaintDescription>
                        <TechnicianNotes>{Job.TechnicianNotes}</TechnicianNotes>
                    </CodesAndCommentsExpanded>"));
        }
    }
}
