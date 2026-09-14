using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using ReconciliationContracts = Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Contracts.Reconciliation
{
    public class LaborExpenseTest
    {

        [Fact]
        public void WhenSelfReferencing_Equals_ReturnsTrue()
        {
            var foo = AutoFaker.Generate<ReconciliationContracts.LaborExpense>();
            Assert.True(foo.Equals(foo));
        }

        [Fact]
        public void WhenPropertiesAreEquivalent_Equals_ReturnsTrue()
        {
            var x = new ReconciliationContracts.LaborExpense
            {
                Amount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 3.50M
                }
            };

            var y = new ReconciliationContracts.LaborExpense
            {
                Amount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 3.50M
                }
            };

            Assert.True(x.Equals(y));
        }

        [Fact]
        public void WhenPropertiesAreNotEquivalent_Equals_ReturnsFalse()
        {
            var x = new ReconciliationContracts.LaborExpense
            {
                Amount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 3.49M
                }
            };

            var y = new ReconciliationContracts.LaborExpense
            {
                Amount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 3.50M
                }
            };

            Assert.False(x.Equals(y));
        }

        [Fact]
        public void WhenOtherIsNull_Equals_ReturnsFalse()
        {
            var foo = AutoFaker.Generate<ReconciliationContracts.LaborExpense>();
            Assert.False(foo.Equals(null));
        }
    }
}
