using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Mappers.RepairOrder._6_0_0;
using Karmak.Integrations.Volvo.React.RepairOrders;
using NSubstitute;
using Xunit;

namespace Karmak.Integrations.Volvo.React.Tests.Mappers.RepairOrder._6_0_0
{
    public class VolvoRepairOrderJobBuilderTests
    {
        private readonly VolvoSettings _volvoSettings;
        private readonly VolvoRepairOrderState _roState;
        private readonly VolvoRepairOrderJobBuilder _builder;
        private readonly RepairOrderSnapshot _repairOrder;
        private readonly RepairOrderSnapshot _repairOrderNoTasks;

        public VolvoRepairOrderJobBuilderTests()
        {
            _repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "RO123",
                CustomerPONumber = "PO456",
                RepairOrderStatus = "OPEN",
                KeyTag = "KEY123",
                DatabaseVersion = "1.0.0",
                BillingCustomer = new Customer
                {
                    CustomerTypeCode = "RETAIL",
                    IndustryType = "RETAIL",
                    CustomerKey = "CUST1"
                },
                OwningCustomer = new Customer(),
                Vehicle = new Vehicle
                {
                    VIN = "1HGBH41JXMN109186",
                    Year = "2023",
                    Make = "Volvo",
                    Model = "F-150",
                    UnitNumber = "UNIT001"
                },
                MeterReading = 50000,
                MeterType = "Miles",
                TimeZone = -6,
                Tasks = new List<RepairOrderTask>()
                {
                    new RepairOrderTask
                    {
                        TaskNumber = 1,
                        AlternateBillingCustomerKey = null,
                        CauseDescription = "Test Cause",
                        ComplaintDescription = "Test Complaint",
                        CorrectionDescription = "Test Correction",
                        LaborEntries = new List<Labor>
                        {
                            new Labor
                            {
                                DateTimeIn = DateTime.UtcNow.AddHours(-2),
                                DateTimeOut = DateTime.UtcNow.AddHours(-1),
                                TechnicianNumber = 1,
                                TotalHours = 2,
                                ExtendedPrice = 200
                            },
                            new Labor
                            {
                                DateTimeIn = DateTime.UtcNow.AddHours(-1),
                                DateTimeOut = DateTime.UtcNow,
                                TechnicianNumber = 2,
                                TotalHours = 1,
                                ExtendedPrice = 100
                            }
                        }
                    }
                }
            };

            _repairOrderNoTasks = new RepairOrderSnapshot
            {
                RepairOrderNumber = "RO123",
                CustomerPONumber = "PO456",
                RepairOrderStatus = "OPEN",
                KeyTag = "KEY123",
                DatabaseVersion = "1.0.0",
                BillingCustomer = new Customer
                {
                    CustomerTypeCode = "RETAIL",
                    IndustryType = "RETAIL",
                    CustomerKey = "CUST1"
                },
                OwningCustomer = new Customer(),
                Vehicle = new Vehicle
                {
                    VIN = "1HGBH41JXMN109186",
                    Year = "2023",
                    Make = "Volvo",
                    Model = "F-150",
                    UnitNumber = "UNIT001"
                },
                MeterReading = 50000,
                MeterType = "Miles",
                TimeZone = -6,
                Tasks = new List<RepairOrderTask>()
            };

            _volvoSettings = Substitute.For<VolvoSettings>();
            _volvoSettings.InterfaceOptions = new InterfaceOptions
            {
                WarrantyCustomers = new[] { "WARRANTY1" },
                EspCustomers = new[] { "ESP1" },
                AwaCustomers = new[] { "AWA1" },
                InternalPolicyCustomers = new[] { "INTERNAL1" }
            };

            _roState = Substitute.For<VolvoRepairOrderState>();
            _roState.RepairOrderStatusState = new VolvoRepairOrderStatusState
            {
                VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
            };

            var partsBuilder = new VolvoRepairOrderPartsBuilder(-6);
            var laborBuilder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);

            _builder = new VolvoRepairOrderJobBuilder(_repairOrder, _volvoSettings, _roState, partsBuilder, laborBuilder);
        }

        [Fact]
        public void BuildJob_WhenTaskIsDeclined_ReturnsDeclinedJob()
        {
            // Arrange
            var task = CreateRepairOrderTask();
            _roState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 1,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.DECLINED
            });

            // Act
            var result = _builder.BuildVolvoJobs();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(VolvoRepairOrderTaskStatus.DECLINED, result[0].jobStatusCode);
            Assert.Equal("00", result[0].jobNumberString);
        }

        [Fact]
        public void BuildJob_WhenTaskIsNotIncluded_ReturnsEmpty()
        {
            // Arrange
            var partsBuilder = new VolvoRepairOrderPartsBuilder(-6);
            var laborBuilder = new VolvoRepairOrderLaborBuilder(_repairOrderNoTasks, _volvoSettings);
            VolvoRepairOrderJobBuilder noTaskBuilder = new VolvoRepairOrderJobBuilder(_repairOrderNoTasks, _volvoSettings, _roState, partsBuilder, laborBuilder);
            _roState.RepairOrderStatusState.VolvoRoStatus = VolvoRepairOrderStatus.ARRIVED;

            // Act
            var result = noTaskBuilder.BuildVolvoJobs();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void BuildJob_WhenTaskIsIncluded_ReturnsVolvoJob()
        {
            // Arrange
            _roState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 1,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
            });

            // Act
            var result = _builder.BuildVolvoJobs();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("00", result[0].jobNumberString);
            Assert.Equal("1", result[0].lineNumber);
            Assert.Equal(VolvoRepairOrderTaskStatus.IN_PROGRESS, result[0].jobStatusCode);
        }

        [Theory]
        [InlineData(VolvoRepairOrderStatus.ARRIVED, true)]
        [InlineData(VolvoRepairOrderStatus.CANCELED, true)]
        [InlineData(VolvoRepairOrderStatus.IN_PROGRESS, false)]
        public void ShouldIncludeThisTask_WithDifferentRoStatuses_ReturnsExpectedResult(string roStatus, bool expected)
        {
            // Arrange
            var task = CreateRepairOrderTask();

            // Act
            var result = _builder.shouldDeleteThisTask(roStatus);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ShouldIncludeThisTask_WithUnknownOperationId_ReturnsFalse()
        {
            // Arrange
            var task = CreateRepairOrderTask();

            // Act
            var result = _builder.shouldIncludeThisTask(false, OperationIds.Unknown);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(false, OperationIds.Customer, true)]
        [InlineData(false, OperationIds.Warranty, false)]
        [InlineData(true, OperationIds.Warranty, true)]
        public void ShouldIncludeThisTask_WithSecondaryRoAndOperationId_ReturnsExpectedResult(bool isSecondaryRO, string operationId, bool expected)
        {
            // Arrange
            var task = CreateRepairOrderTask();

            // Act
            var result = _builder.shouldIncludeThisTask(isSecondaryRO, operationId);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void BuildJob_WhenTaskHasMultipleLaborRecords_SplitsIntoMultipleJobs()
        {
            // Arrange
            var repairOrderWithMultipleLabor = new RepairOrderSnapshot
            {
                RepairOrderNumber = "RO123",
                CustomerPONumber = "PO456",
                RepairOrderStatus = "OPEN",
                KeyTag = "KEY123",
                DatabaseVersion = "1.0.0",
                BillingCustomer = new Customer
                {
                    CustomerTypeCode = "RETAIL",
                    IndustryType = "RETAIL",
                    CustomerKey = "CUST1"
                },
                OwningCustomer = new Customer(),
                Vehicle = new Vehicle
                {
                    VIN = "1HGBH41JXMN109186",
                    Year = "2023",
                    Make = "Volvo",
                    Model = "F-150",
                    UnitNumber = "UNIT001"
                },
                MeterReading = 50000,
                MeterType = "Miles",
                TimeZone = -6,
                Tasks = new List<RepairOrderTask>
                {
                    new RepairOrderTask
                    {
                        TaskNumber = 1,
                        AlternateBillingCustomerKey = null,
                        CauseDescription = "Test Cause",
                        ComplaintDescription = "Test Complaint",
                        CorrectionDescription = "Test Correction",
                        LaborEntries = new List<Labor>
                        {
                            new Labor
                            {
                                DateTimeIn = DateTime.UtcNow.AddHours(-3),
                                DateTimeOut = DateTime.UtcNow.AddHours(-2),
                                TechnicianNumber = 1,
                                TotalHours = 1.1m,
                                ExtendedPrice = 100
                            },
                            new Labor
                            {
                                DateTimeIn = DateTime.UtcNow.AddHours(-2),
                                DateTimeOut = DateTime.UtcNow.AddHours(-1),
                                TechnicianNumber = 2,
                                TotalHours = 2.2m,
                                ExtendedPrice = 200
                            },
                            new Labor
                            {
                                DateTimeIn = DateTime.UtcNow.AddHours(-1),
                                DateTimeOut = DateTime.UtcNow,
                                TechnicianNumber = 3,
                                TotalHours = 3.3m,
                                ExtendedPrice = 300
                            }
                        }
                    }
                }
            };

            var partsBuilder = new VolvoRepairOrderPartsBuilder(-6);
            var laborBuilder = new VolvoRepairOrderLaborBuilder(repairOrderWithMultipleLabor, _volvoSettings);
            var builder = new VolvoRepairOrderJobBuilder(repairOrderWithMultipleLabor, _volvoSettings, _roState, partsBuilder, laborBuilder);

            _roState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 1,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
            });

            // Act
            var result = builder.BuildVolvoJobs();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("00", result[0].jobNumberString);
            Assert.Equal("01", result[1].jobNumberString);
            Assert.Equal("02", result[2].jobNumberString);
            Assert.Equal("1", result[0].serviceLabor.serviceTechnicianParty[0].id);
            Assert.Equal("2", result[1].serviceLabor.serviceTechnicianParty[0].id);
            Assert.Equal("3", result[2].serviceLabor.serviceTechnicianParty[0].id);
            Assert.Equal(11, result[0].serviceLabor.laborActualHoursNumeric);
            Assert.Equal(22, result[1].serviceLabor.laborActualHoursNumeric);
            Assert.Equal(33, result[2].serviceLabor.laborActualHoursNumeric);
            Assert.All(result, job => Assert.Equal(VolvoRepairOrderTaskStatus.IN_PROGRESS, job.jobStatusCode));
        }

        [Fact]
        public void BuildCodesAndComments_MaliciousDescriptions_SanitizesCorrectly()
        {
            // Arrange
            _repairOrder.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    CauseDescription = "Engine<script>alert('xss')</script>failure",
                    ComplaintDescription = "Car won't start'; DROP TABLE--",
                    CorrectionDescription = "Replaced<img src=x>battery",
                    LaborEntries = new List<Labor>(),
                    Parts = new List<Part>(),
                    MiscCharges = new List<MiscCharge>()
                }
            };

            var partsBuilder = new VolvoRepairOrderPartsBuilder(-6);
            var laborBuilder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var builder = new VolvoRepairOrderJobBuilder(_repairOrder, _volvoSettings, _roState, partsBuilder, laborBuilder);

            // Act
            var result = builder.BuildVolvoJobs();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            var codesAndComments = result[0].codesAndCommentsExpanded;
            Assert.NotNull(codesAndComments);

            Assert.DoesNotContain("<", codesAndComments.causeDescription ?? "");
            Assert.DoesNotContain(">", codesAndComments.causeDescription ?? "");

            Assert.DoesNotContain(";", codesAndComments.complaintDescription ?? "");

            Assert.DoesNotContain("<", codesAndComments.correctionDescription ?? "");
            Assert.DoesNotContain(">", codesAndComments.correctionDescription ?? "");
        }

        [Fact]
        public void BuildCodesAndComments_ControlCharacters_RemovesControlChars()
        {
            // Arrange
            _repairOrder.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    CauseDescription = "Test\x00 Description",
                    ComplaintDescription = "Complaint\x1FText",
                    CorrectionDescription = "Fixed\x00Issue",
                    LaborEntries = new List<Labor>(),
                    Parts = new List<Part>(),
                    MiscCharges = new List<MiscCharge>()
                }
            };

            var partsBuilder = new VolvoRepairOrderPartsBuilder(-6);
            var laborBuilder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var builder = new VolvoRepairOrderJobBuilder(_repairOrder, _volvoSettings, _roState, partsBuilder, laborBuilder);

            // Act
            var result = builder.BuildVolvoJobs();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            var codesAndComments = result[0].codesAndCommentsExpanded;
            Assert.NotNull(codesAndComments);

            Assert.Equal("Test Description", codesAndComments.causeDescription);
            Assert.Equal("ComplaintText", codesAndComments.complaintDescription);
            Assert.Equal("FixedIssue", codesAndComments.correctionDescription);
        }

        private RepairOrderTask CreateRepairOrderTask()
        {
            return new RepairOrderTask
            {
                TaskNumber = 1,
                AlternateBillingCustomerKey = null,
                CauseDescription = "Test Cause",
                ComplaintDescription = "Test Complaint",
                CorrectionDescription = "Test Correction",
                LaborEntries = new List<Labor>
                {
                    new Labor
                    {
                        DateTimeIn = DateTime.UtcNow.AddHours(-2),
                        DateTimeOut = DateTime.UtcNow.AddHours(-1)
                    }
                }
            };
        }
    }
}