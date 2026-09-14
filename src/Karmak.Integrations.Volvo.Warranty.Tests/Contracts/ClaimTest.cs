using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Contracts
{
    public class ClaimTest
    {
        [Fact]
        public void WhenClaimHasCausalPart_It_ReturnsCausalPartIdentifier()
        {
            var partIdentifier = Guid.NewGuid().ToString();
            var claim = new Claim
            {
                RepairOrder = new RepairOrder
                {
                    Jobs = new[] {
                        new Job {
                            PartExpenses = new [] {
                                new PartExpense {
                                    Identifier = partIdentifier,
                                    IsCausalPart = true
                                },
                                new PartExpense {
                                    Identifier = Guid.NewGuid().ToString()
                                },
                            }
                        }
                    }
                }
            };

            Assert.Equal(partIdentifier, claim.CausalPartIdentifier);
        }

        [Fact]
        public void WhenClaimDoesNotHaveCausalPart_It_ReturnsNullCausalPartIdentifier()
        {
            var claim = new Claim
            {
                RepairOrder = new RepairOrder
                {
                    Jobs = new[] {
                        new Job {
                            PartExpenses = new [] {
                                new PartExpense {
                                    Identifier = Guid.NewGuid().ToString()
                                },
                                new PartExpense {
                                    Identifier = Guid.NewGuid().ToString()
                                },
                            }
                        }
                    }
                }
            };

            Assert.Null(claim.CausalPartIdentifier);
        }
    }
}
