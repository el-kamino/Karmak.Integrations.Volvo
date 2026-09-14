using FluentValidation;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.Warranty.Validators;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Validators
{
    public class WarrantyRepairOrderValidatorTest
    {
        [Fact]
        public void WhenTheWarrantyCustomersIncludesBillingCustomerKey_It_IsValid()
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "ABCDEFG",
                OriginalRepairOrderNumber = "1234567",
                BillingCustomer = new Customer
                {
                    CustomerKey = "010101"
                },
                InvoiceDate = DateTime.Now
            };

            var result = new WarrantyRepairOrderValidator().Validate(new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = {
                    [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] = new[] {
                        "010101"
                    }
                }
            });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void WhenMissingTheWarrantyCustomersRootContext_It_IsNotValid()
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "ABCDEFG",
                OriginalRepairOrderNumber = "1234567",
                BillingCustomer = new Customer
                {
                    CustomerKey = "010101"
                },
                InvoiceDate = DateTime.Now
            };

            var result = new WarrantyRepairOrderValidator().Validate(repairOrder);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void WhenTheWarrantyCustomersContextIsNull_It_IsNotValid()
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "ABCDEFG",
                OriginalRepairOrderNumber = "1234567",
                BillingCustomer = new Customer
                {
                    CustomerKey = "010101"
                },
                InvoiceDate = DateTime.Now
            };

            var result = new WarrantyRepairOrderValidator().Validate(new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = { [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] = null }
            });

            Assert.False(result.IsValid);
        }

        [Fact]
        public void WhenTheWarrantyCustomersContextExcludesBillingCustomerCompanyName_It_IsNotValid()
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "ABCDEFG",
                OriginalRepairOrderNumber = "1234567",
                BillingCustomer = new Customer
                {
                    CustomerKey = "010101"
                },
                InvoiceDate = DateTime.Now
            };

            var result = new WarrantyRepairOrderValidator().Validate(new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = { [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] = Array.Empty<string>() }
            });

            Assert.False(result.IsValid);
        }

        [Fact]
        public void WhenMissingTheBillingCustomer_It_IsNotValid()
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "ABCDEFG",
                OriginalRepairOrderNumber = "1234567",
                BillingCustomer = null,
                InvoiceDate = DateTime.Now
            };

            var result = new WarrantyRepairOrderValidator().Validate(new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = { [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] = new[] { "Ted Nugent" } }
            });

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingTheWarrantyCustomersBillingCustomerCompanyName_It_IsNotValid(string value)
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "ABCDEFG",
                OriginalRepairOrderNumber = "1234567",
                BillingCustomer = new Customer
                {
                    CustomerKey = value
                },
                InvoiceDate = DateTime.Now
            };

            var result = new WarrantyRepairOrderValidator().Validate(new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = { [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] = Array.Empty<string>() }
            });

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingTheRepairOrderNumber_It_IsNotValid(string value)
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = value,
                OriginalRepairOrderNumber = "1234567",
                BillingCustomer = new Customer
                {
                    CustomerKey = "010101"
                },
                InvoiceDate = DateTime.Now
            };

            var result = new WarrantyRepairOrderValidator().Validate(new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = { [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] = new[] { "010101" } }
            });

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WhenMissingTheOriginalRepairOrderNumber_It_IsValid(string value)
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "ABCDEFG",
                OriginalRepairOrderNumber = value,
                BillingCustomer = new Customer
                {
                    CustomerKey = "010101"
                },
                InvoiceDate = DateTime.Now
            };

            var result = new WarrantyRepairOrderValidator().Validate(new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = { [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] = new[] { "010101" } }
            });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void WhenMissingTheInvoiceDate_It_IsNotValid()
        {
            var repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "ABCDEFG",
                OriginalRepairOrderNumber = "1234567",
                BillingCustomer = new Customer
                {
                    CustomerKey = "010101"
                },
                InvoiceDate = null
            };

            var result = new WarrantyRepairOrderValidator().Validate(new ValidationContext<RepairOrderSnapshot>(repairOrder)
            {
                RootContextData = { [WarrantyRepairOrderValidator.WarrantyCustomersContextKey] = new[] { "010101" } }
            });

            Assert.False(result.IsValid);
        }
    }
}
