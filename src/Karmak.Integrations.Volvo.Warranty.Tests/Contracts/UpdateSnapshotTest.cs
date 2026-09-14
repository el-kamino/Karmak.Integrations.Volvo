using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using WarrantyReconciliation = Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Contracts
{
    public class UpdateSnapshotTest
    {

        [Fact]
        public void WhenExpenses_AreNotEqual_TheUpdatesArentDuplicates()
        {
            var foo = new UpdateSnapshot
            {
                Id = "9/21/2021 3:01:12 PM-9/20/2021 12:00:00 AM-01869-3258-3",
                OemId = "9/20/2021 12:00:00 AM-01869-3258-3",
                DealerCode = "01869",
                CreatedDateTime = Convert.ToDateTime("2021-09-21T15:01:12.8653571+00:00"),
                ProcessedByFusionDateTime = null,
                Type = UpdateType.Reconciliation,
                RepairOrderNumber = "3258",
                ProcessDate = Convert.ToDateTime("2021-09-20T00:00:00"),
                ApprovedAmount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 54.27M
                },
                Status = new UpdateStatus
                {
                    Code = "A",
                    Description = "Approved - Other Repairs"
                },
                Exceptions = new List<UpdateException>(),
                Deductibles = new List<Deductible>(),
                Taxes = new List<Tax>(),
                LaborAmount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 54.27M
                },
                PartsAmount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 0.00M
                },
                OtherAmount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 0.00M
                },
                PartExpenses = Array.Empty<WarrantyReconciliation.PartExpense>(),
                LaborExpenses = new List<WarrantyReconciliation.LaborExpense> {
                    {
                        new WarrantyReconciliation.LaborExpense {
                            OperationId = "21S26B",
                            Description = "",
                            Hours = new Quantity {
                                Type = UnitOfMeasureType.Hours,
                                Value = 0.5M
                            },
                            RequestedHours = null,
                            Amount = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 54.27M
                            },
                        }
                    }
                },
                MiscellaneousExpenses = Array.Empty<WarrantyReconciliation.MiscellaneousExpense>(),
            };

            var foo2 = new UpdateSnapshot
            {
                Id = "9/21/2021 3:01:12 PM-9/20/2021 12:00:00 AM-01869-3258-3",
                OemId = "9/20/2021 12:00:00 AM-01869-3258-3",
                DealerCode = "01869",
                CreatedDateTime = Convert.ToDateTime("2021-09-21T15:01:12.8653571+00:00"),
                ProcessedByFusionDateTime = null,
                Type = UpdateType.Reconciliation,
                RepairOrderNumber = "3258",
                ProcessDate = Convert.ToDateTime("2021-09-20T00:00:00"),
                ApprovedAmount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 54.27M
                },
                Status = new UpdateStatus
                {
                    Code = "A",
                    Description = "Approved - Other Repairs"
                },
                Exceptions = new List<UpdateException>(),
                Deductibles = new List<Deductible>(),
                Taxes = new List<Tax>(),
                LaborAmount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 54.27M
                },
                PartsAmount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 0.00M
                },
                OtherAmount = new Money
                {
                    Currency = CurrencyCode.USD,
                    Value = 0.00M
                },
                PartExpenses = Array.Empty<WarrantyReconciliation.PartExpense>(),
                LaborExpenses = new List<WarrantyReconciliation.LaborExpense> {
                    {
                        new WarrantyReconciliation.LaborExpense {
                            OperationId = "21S26B",
                            Description = "",
                            Hours = new Quantity {
                                Type = UnitOfMeasureType.Hours,
                                Value = 0.5M
                            },
                            RequestedHours = null,
                            Amount = new Money {
                                Currency = CurrencyCode.USD,
                                Value = 54.27M
                            },
                        }
                    }
                },
                MiscellaneousExpenses = AutoFaker.Generate<WarrantyReconciliation.MiscellaneousExpense>(3),
            };

            Assert.False(foo.IsDuplicate(foo2));
        }

        [Fact]
        public void WhenExpenses_AreEmpty_ItDoesNotThrow()
        {
            var foo = CreateSnapshot();
            var foo2 = CreateSnapshot();

            Assert.True(foo.IsDuplicate(foo2));
        }

        [Fact]
        public void IsDuplicate_ReturnsTrue_WhenUpdatesAreEqual()
        {
            var exceptions = AutoFaker.Generate<UpdateException>(3);
            var deductibles = AutoFaker.Generate<Deductible>(3);
            var taxes = AutoFaker.Generate<Tax>(3);
            var partExpenses = AutoFaker.Generate<WarrantyReconciliation.PartExpense>(3);
            var laborA = AutoFaker.Generate<WarrantyReconciliation.LaborExpense>();
            var laborB = AutoFaker.Generate<WarrantyReconciliation.LaborExpense>();
            var xLaborExpenses = new List<WarrantyReconciliation.LaborExpense> { laborA, laborB };
            var yLaborExpenses = new List<WarrantyReconciliation.LaborExpense> { laborB, laborA };
            var miscellaneousExpenses = AutoFaker.Generate<WarrantyReconciliation.MiscellaneousExpense>(3);

            var x = new UpdateSnapshot
            {
                Id = "123",
                OemId = "456",
                DealerCode = "dealerCode",
                CreatedDateTime = DateTimeOffset.MinValue,
                ProcessedByFusionDateTime = null,
                RepairOrderNumber = "99",
                Type = UpdateType.Reconciliation,
                ProcessDate = DateTime.MinValue,
                ApprovedAmount = CreateMoney(10),
                Status = new UpdateStatus
                {
                    Code = "statusCode",
                    Description = "statusDescription"
                },
                Exceptions = exceptions,
                Deductibles = deductibles,
                Taxes = taxes,
                LaborAmount = CreateMoney(2),
                PartsAmount = CreateMoney(3),
                OtherAmount = CreateMoney(5),
                PartExpenses = partExpenses,
                LaborExpenses = xLaborExpenses,
                MiscellaneousExpenses = miscellaneousExpenses
            };

            var y = new UpdateSnapshot
            {
                Id = "123",
                OemId = "456",
                DealerCode = "dealerCode",
                CreatedDateTime = DateTimeOffset.MinValue,
                ProcessedByFusionDateTime = null,
                RepairOrderNumber = "99",
                Type = UpdateType.Reconciliation,
                ProcessDate = DateTime.MinValue,
                ApprovedAmount = CreateMoney(10),
                Status = new UpdateStatus
                {
                    Code = "statusCode",
                    Description = "statusDescription"
                },
                Exceptions = exceptions,
                Deductibles = deductibles,
                Taxes = taxes,
                LaborAmount = CreateMoney(2),
                PartsAmount = CreateMoney(3),
                OtherAmount = CreateMoney(5),
                PartExpenses = partExpenses,
                LaborExpenses = yLaborExpenses,
                MiscellaneousExpenses = miscellaneousExpenses
            };

            Assert.True(y.IsDuplicate(x));
        }

        [Fact]
        public void IsDuplicate_ReturnsFalse_WhenUpdatesAreDifferent()
        {
            var x = new UpdateSnapshot { OemId = "12345" };
            var y = new UpdateSnapshot { OemId = "67890" };

            Assert.False(y.IsDuplicate(x));
        }

        [Fact]
        public void IsDuplicate_ReturnsTrue_WhenUpdatesDifferById()
        {
            var x = new UpdateSnapshot
            {
                OemId = "12345",
                Id = "123"
            };
            var y = new UpdateSnapshot
            {
                OemId = "12345",
                Id = "456"
            };

            Assert.True(y.IsDuplicate(x));
        }

        [Fact]
        public void IsDuplicate_ReturnsTrue_WhenUpdatesDifferByCreatedDateTime()
        {
            var laborA = new WarrantyReconciliation.LaborExpense
            {
                OperationId = "foobar"
            };
            var laborB = AutoFaker.Generate<WarrantyReconciliation.LaborExpense>();
            var laborC = new WarrantyReconciliation.LaborExpense
            {
                OperationId = "foobar"
            };
            var xLaborExpenses = new List<WarrantyReconciliation.LaborExpense> { laborA, laborB };
            var yLaborExpenses = new List<WarrantyReconciliation.LaborExpense> { laborB, laborC };

            var x = new UpdateSnapshot
            {
                OemId = "12345",
                CreatedDateTime = DateTimeOffset.MinValue,
                LaborExpenses = xLaborExpenses
            };
            var y = new UpdateSnapshot
            {
                OemId = "12345",
                CreatedDateTime = DateTimeOffset.MaxValue,
                LaborExpenses = yLaborExpenses
            };

            Assert.True(y.IsDuplicate(x));
        }

        [Fact]
        public void IsDuplicate_ReturnsTrue_WhenUpdatesDifferByProcessByFusionDateTime()
        {
            var x = new UpdateSnapshot
            {
                OemId = "12345",
                ProcessedByFusionDateTime = DateTimeOffset.MaxValue
            };
            var y = new UpdateSnapshot
            {
                OemId = "12345",
                ProcessedByFusionDateTime = null
            };

            Assert.True(y.IsDuplicate(x));
        }

        private Money CreateMoney(decimal value) =>
            new Money
            {
                Value = value,
                Currency = CurrencyCode.USD
            };

        private UpdateSnapshot CreateSnapshot() => new UpdateSnapshot
        {
            Id = "9/21/2021 3:01:12 PM-9/20/2021 12:00:00 AM-01869-3258-3",
            OemId = "9/20/2021 12:00:00 AM-01869-3258-3",
            DealerCode = "01869",
            CreatedDateTime = Convert.ToDateTime("2021-09-21T15:01:12.8653571+00:00"),
            ProcessedByFusionDateTime = null,
            Type = UpdateType.Reconciliation,
            RepairOrderNumber = "3258",
            ProcessDate = Convert.ToDateTime("2021-09-20T00:00:00"),
            ApprovedAmount = new Money { Currency = CurrencyCode.USD, Value = 54.27M },
            Status = new UpdateStatus { Code = "A", Description = "Approved - Other Repairs" },
            Exceptions = new List<UpdateException>(),
            Deductibles = new List<Deductible>(),
            Taxes = new List<Tax>(),
            LaborAmount = new Money { Currency = CurrencyCode.USD, Value = 54.27M },
            PartsAmount = new Money { Currency = CurrencyCode.USD, Value = 0.00M },
            OtherAmount = new Money { Currency = CurrencyCode.USD, Value = 0.00M },
            PartExpenses = Array.Empty<WarrantyReconciliation.PartExpense>(),
            LaborExpenses = new List<WarrantyReconciliation.LaborExpense> {
                new WarrantyReconciliation.LaborExpense {
                    OperationId = "21S26B",
                    Description = "",
                    Hours = new Quantity { Type = UnitOfMeasureType.Hours, Value = 0.5M },
                    RequestedHours = null,
                    Amount = new Money { Currency = CurrencyCode.USD, Value = 54.27M },
                }
            },
            MiscellaneousExpenses = Array.Empty<WarrantyReconciliation.MiscellaneousExpense>(),
        };
    }
}
