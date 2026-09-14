using System.Xml.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing;
using Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim.Data;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim
{
    public class AppealTranslationToVolvoBodXmlTest
    {
        private static readonly SubmitClaimTranslator _submitClaimTranslator = new SubmitClaimTranslator(SubmitClaimJobTranslatorFactory.Create(), "Test");

        public class AppealTranslations
        {

            [Theory]
            [ClassData(typeof(ClaimTypesWithEmptyAppealCodes))]
            public void WhenTranslatingAndSerializing_WithEmptyAppealActionCode_DoesNotIncludeAppealActionCodeInLabor(string claimType, string appealActionCode)
            {
                var claim = FakeClaim.Generate();
                claim.Type = claimType;
                claim.InAppeal = true;
                claim.FleetAccount = null;
                claim.RepairOrder.Jobs = new[] {
                    new Job {
                        LaborExpenses = new[] {
                            new LaborExpense {
                                Identifier = "1234",
                                AppealCode = appealActionCode,
                            }
                        }
                    }
                };

                var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

                xml.AssertNotContainsIn(TestConstants.OWS_NAMESPACE + "ServiceLaborExtended", XElement.Parse($@"
                        <AppealActionCode xmlns=""{TestConstants.STAR_NAMESPACE}"">{appealActionCode}</AppealActionCode>"));
            }

            [Theory]
            [ClassData(typeof(AllClaimTypes))]
            public void WhenTranslatingAndSerializing_It_IncludesAppealActionCodeInLabor(string claimType)
            {
                const string appealCode = "A";

                var claim = FakeClaim.Generate();
                claim.Type = claimType;
                claim.InAppeal = true;
                claim.FleetAccount = null;
                claim.RepairOrder.Jobs = new[] {
                    new Job {
                        LaborExpenses = new[] {
                            new LaborExpense {
                                Identifier = "1234",
                                AppealCode = appealCode,
                            }
                        }
                    }
                };

                var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

                xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "ServiceLaborExtended", XElement.Parse($@"
                        <AppealActionCode xmlns=""{TestConstants.STAR_NAMESPACE}"">{appealCode}</AppealActionCode>"));
            }


            [Theory]
            [ClassData(typeof(ClaimTypesWithEmptyAppealCodes))]
            public void WhenTranslatingAndSerializing_WithEmptyOrNullAppealActionCode_DoesNotIncludeAppealActionCodeInParts(string claimType, string appealActionCode)
            {
                var claim = FakeClaim.Generate();
                claim.Type = claimType;
                claim.InAppeal = true;
                claim.FleetAccount = null;
                claim.RepairOrder.Jobs = new[] {
                    new Job {
                        PartExpenses = new[] {
                            new PartExpense {
                                Identifier = "1234",
                                AppealCode = appealActionCode,
                                Quantity = new Quantity {
                                    Type = UnitOfMeasureType.Count,
                                    Value = 1
                                },
                                CorePrice = new Money {
                                    Currency = CurrencyCode.USD,
                                    Value = 1.00m
                                }
                            }
                        }
                    }
                };

                var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

                xml.AssertNotContainsIn(TestConstants.OWS_NAMESPACE + "ServicePartsExtended", XElement.Parse($@"
                        <AppealActionCode xmlns=""{TestConstants.STAR_NAMESPACE}"">{appealActionCode}</AppealActionCode>"));
            }

            [Theory]
            [ClassData(typeof(AllClaimTypes))]
            public void WhenTranslatingAndSerializing_It_IncludesAppealActionCodeInParts(string claimType)
            {
                const string appealCode = "A";

                var claim = FakeClaim.Generate();
                claim.Type = claimType;
                claim.InAppeal = true;
                claim.FleetAccount = null;
                claim.RepairOrder.Jobs = new[] {
                    new Job {
                        PartExpenses = new[] {
                            new PartExpense {
                                Identifier = "1234",
                                AppealCode = appealCode,
                                Quantity = new Quantity {
                                    Type = UnitOfMeasureType.Count,
                                    Value = 1
                                },
                                CorePrice = new Money {
                                    Currency = CurrencyCode.USD,
                                    Value = 1.00m
                                }
                            }
                        }
                    }
                };

                var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

                xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "ServicePartsExtended", XElement.Parse($@"
                        <AppealActionCode xmlns=""{TestConstants.STAR_NAMESPACE}"">{appealCode}</AppealActionCode>"));
            }

            [Theory]
            [ClassData(typeof(ClaimTypesWithEmptyAppealCodes))]
            public void WhenTranslatingAndSerializing_WithEmptyAppealActionCode_DoesNotIncludeAppealActionCodeInMiscellaneous(string claimType, string appealActionCode)
            {
                var claim = FakeClaim.Generate();
                claim.Type = claimType;
                claim.InAppeal = true;
                claim.FleetAccount = null;
                claim.RepairOrder.Jobs = new[] {
                    new Job {
                        MiscellaneousExpenses = new[] {
                            new MiscellaneousExpense {
                                Identifier = "1234",
                                AppealCode = appealActionCode,
                                Quantity = new Quantity {
                                    Value = 0m,
                                    Type = UnitOfMeasureType.Days
                                },
                                UnitPrice = new Money {
                                    Value = 0m,
                                    Currency = CurrencyCode.USD
                                }
                            }
                        }
                    }
                };

                var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

                xml.AssertNotContainsIn(TestConstants.OWS_NAMESPACE + "ServiceComponentsExtended", XElement.Parse($@"
                        <AppealActionCode xmlns=""{TestConstants.STAR_NAMESPACE}"">{appealActionCode}</AppealActionCode>"));
            }

            [Theory]
            [ClassData(typeof(AllClaimTypes))]
            public void WhenTranslatingAndSerializing_It_IncludesAppealActionCodeInMiscellaneous(string claimType)
            {
                const string appealCode = "A";

                var claim = FakeClaim.Generate();
                claim.Type = claimType;
                claim.InAppeal = true;
                claim.FleetAccount = null;
                claim.RepairOrder.Jobs = new[] {
                    new Job {
                        MiscellaneousExpenses = new[] {
                            new MiscellaneousExpense {
                                Identifier = "1234",
                                AppealCode = appealCode,
                                UnitPrice = new Money {
                                    Value = 1m,
                                    Currency = CurrencyCode.USD
                                },
                                Quantity = new Quantity {
                                    Value = 0m,
                                    Type = UnitOfMeasureType.Days
                                }
                            }
                        }
                    }
                };

                var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

                xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "ServiceComponentsExtended", XElement.Parse($@"
                        <AppealActionCode xmlns=""{TestConstants.STAR_NAMESPACE}"">{appealCode}</AppealActionCode>"));
            }

            [Theory]
            [ClassData(typeof(AllClaimTypes))]
            public void WhenTranslatingAndSerializing_It_IncludesAppealReasonCode(string claimType)
            {
                const string appealReasonCode = "foobar";

                var claim = FakeClaim.Generate();
                claim.Type = claimType;
                claim.InAppeal = true;
                claim.FleetAccount = null;
                claim.RepairOrder.Jobs = new[] {
                    new Job {
                        AppealReasonCode = appealReasonCode
                    }
                };

                var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

                xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "WarrantyClaimExtended", XElement.Parse($@"
                        <AppealReasonCode xmlns=""{TestConstants.STAR_NAMESPACE}"">{appealReasonCode}</AppealReasonCode>"));
            }

            [Theory]
            [ClassData(typeof(AllClaimTypes))]
            public void WhenTranslatingAndSerializing_It_IncludesAppealComments(string claimType)
            {
                const string appealComments = "foobarqux";

                var claim = FakeClaim.Generate();
                claim.Type = claimType;
                claim.InAppeal = true;
                claim.FleetAccount = null;
                claim.RepairOrder.Jobs = new[] {
                    new Job {
                        AppealComments = appealComments
                    }
                };

                var xml = _submitClaimTranslator.TranslateAndSerialize(new List<Claim>() { claim });

                xml.AssertContainsIn(TestConstants.OWS_NAMESPACE + "WarrantyClaimExtended", XElement.Parse($@"
                        <AppealComments xmlns=""{TestConstants.OWS_NAMESPACE}"">{appealComments}</AppealComments>"));
            }
        }
    }
}
