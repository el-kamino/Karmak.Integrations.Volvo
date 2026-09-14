using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Karmak.Integrations.Volvo.React.Tests.RepairOrders.V6_0_0
{
    public class VolvoRepairOrderStatusEvaluator_StatusTests
    {
        private readonly NullLogger<VolvoRepairOrderStatusEvaluator> _mockLogger;
        private readonly IKarmakBlobClient _mockBlobClient;
        private readonly Dictionary<string, string> _metaData;

        public VolvoRepairOrderStatusEvaluator_StatusTests()
        {
            _mockLogger = new NullLogger<VolvoRepairOrderStatusEvaluator>();
            _mockBlobClient = Substitute.For<IKarmakBlobClient>();
            _metaData = new Dictionary<string, string>
            {
                { "RepairOrder.Number", "TEST-RO-123" }
            };
        }

        [Fact]
        public void RepairOrderStatus_ShouldReturnCanceled_WhenRepairOrderIsVoidedAndRoHasNotBeenClosed()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.VOIDED);
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.CANCELED, result.VolvoRoStatus);
        }

        [Fact]
        public void RepairOrderStatus_ShouldReturnUndefined_WhenRepairOrderIsVoidedAndRoHasBeenClosed()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.VOIDED);
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, true);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.UNDEFINED, result.VolvoRoStatus);
        }

        [Fact]
        public void RepairOrderStatus_ShouldReturnUndefined_WhenRepairOrderIsAtDealershipAndWeHaveSentThat()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.VOIDED);
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, true, true);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.UNDEFINED, result.VolvoRoStatus);
        }

        [Theory]
        [InlineData(RepairOrderStatus.CLOSED)]
        [InlineData(RepairOrderStatus.INVOICED)]
        public void RepairOrderStatus_ShouldReturnClosed_WhenRepairOrderIsClosedOrInvoiced(string status)
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(status);
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.CLOSED, result.VolvoRoStatus);
        }

        [Fact]
        public void RepairOrderStatus_ShouldReturnOnHold_WhenAllTasksAreOnHold()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.OPEN);
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = RepairOrderTaskStatus.HOLD,
                LaborEntries = new List<Labor>()
            });
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = RepairOrderTaskStatus.WAITING_FOR_PARTS,
                LaborEntries = new List<Labor>()
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.ON_HOLD, result.VolvoRoStatus);
        }

        [Theory]
        [InlineData(RepairOrderStatus.OPEN, RepairOrderTaskStatus.HOLD)]
        [InlineData(RepairOrderStatus.OPEN, RepairOrderTaskStatus.WAITING_FOR_PARTS)]
        [InlineData(RepairOrderStatus.OPEN, RepairOrderTaskStatus.CLOSED)]
        [InlineData(RepairOrderStatus.OPEN, RepairOrderTaskStatus.QUOTE_DECLINED)]
        public void RepairOrderStatus_ShouldReturnReOpened_WhenOpenRoHasBeenClosedBefore(string roStatus, string taskStatus)
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(roStatus);
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = taskStatus,
                LaborEntries = new List<Labor>()
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, true);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.RE_OPENED, result.VolvoRoStatus);
        }

        [Theory]
        [InlineData(RepairOrderStatus.CLOSED, RepairOrderTaskStatus.HOLD)]
        [InlineData(RepairOrderStatus.CLOSED, RepairOrderTaskStatus.WAITING_FOR_PARTS)]
        [InlineData(RepairOrderStatus.CLOSED, RepairOrderTaskStatus.CLOSED)]
        [InlineData(RepairOrderStatus.CLOSED, RepairOrderTaskStatus.QUOTE_DECLINED)]
        [InlineData(RepairOrderStatus.INVOICED, RepairOrderTaskStatus.HOLD)]
        [InlineData(RepairOrderStatus.INVOICED, RepairOrderTaskStatus.WAITING_FOR_PARTS)]
        [InlineData(RepairOrderStatus.INVOICED, RepairOrderTaskStatus.CLOSED)]
        [InlineData(RepairOrderStatus.INVOICED, RepairOrderTaskStatus.QUOTE_DECLINED)]
        public void RepairOrderStatus_ShouldReturnClosed_WhenClosedOrInvoicedRoHasBeenClosedBefore(string roStatus, string taskStatus)
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(roStatus);
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = taskStatus,
                LaborEntries = new List<Labor>()
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, true);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.CLOSED, result.VolvoRoStatus);
        }


        [Fact]
        public void RepairOrderStatus_ShouldReturnContacted_WhenAllTasksCompleteAndCustomerContacted()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.OPEN);
            repairOrder.CustomerContactedStatus = RepairOrderSubstatus.READY_FOR_PICKUP;
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = RepairOrderTaskStatus.CLOSED,
                LaborEntries = new List<Labor>()
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal( VolvoRepairOrderStatus.CONTACTED, result.VolvoRoStatus);
        }

        [Fact]
        public void RepairOrderStatus_ShouldReturnInProgress_WhenRoHasLaborEntries()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.OPEN);
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = RepairOrderTaskStatus.HOLD,
            });
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 2,
                RepairTaskStatus = RepairOrderTaskStatus.CLOSED,
            });
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 3,
                RepairTaskStatus = RepairOrderTaskStatus.OPEN,
                LaborEntries = new List<Labor> { new Labor() }
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.IN_PROGRESS, result.VolvoRoStatus);
        }

        [Fact]
        public void RepairOrderStatus_ShouldReturnArrived_WhenSubStatusIsAtDealership()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.OPEN);
            repairOrder.SubStatus = RepairOrderSubstatus.AT_DEALERSHIP;
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.ARRIVED, result.VolvoRoStatus);
        }

        [Fact]
        public void RepairOrderStatus_ShouldReturnUndefined_WhenNoStatusMatches()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.OPEN);
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Equal(VolvoRepairOrderStatus.UNDEFINED, result.VolvoRoStatus);
        }

        [Theory]
        [InlineData(RepairOrderTaskStatus.HOLD, VolvoRepairOrderTaskStatus.ON_HOLD)]
        [InlineData(RepairOrderTaskStatus.WAITING_FOR_PARTS, VolvoRepairOrderTaskStatus.ON_HOLD)]
        [InlineData(RepairOrderTaskStatus.CLOSED, VolvoRepairOrderTaskStatus.COMPLETED)]
        [InlineData(RepairOrderTaskStatus.QUOTE_DECLINED, VolvoRepairOrderTaskStatus.DECLINED)]
        public void TaskStatus_ShouldReturnCorrectTaskStatus(string taskStatus, string expectedVolvoStatus)
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.OPEN);
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = taskStatus,
                LaborEntries = new List<Labor>()
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Single(result.TaskStatusStates);
            Assert.Equal(expectedVolvoStatus, result.TaskStatusStates[0].VolvoRoTaskStatus);
        }

        [Theory]
        [InlineData(RepairOrderTaskStatus.HOLD, VolvoRepairOrderTaskStatus.COMPLETED)]
        [InlineData(RepairOrderTaskStatus.WAITING_FOR_PARTS, VolvoRepairOrderTaskStatus.COMPLETED)]
        [InlineData(RepairOrderTaskStatus.CLOSED, VolvoRepairOrderTaskStatus.COMPLETED)]
        [InlineData(RepairOrderTaskStatus.QUOTE_DECLINED, VolvoRepairOrderTaskStatus.DECLINED)]
        public void TaskStatus_ShouldReturnCompletedOrDeclinedIfRoIsClosed(string taskStatus, string expectedVolvoStatus)
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.CLOSED);
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = taskStatus,
                LaborEntries = new List<Labor>()
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);  

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Single(result.TaskStatusStates);
            Assert.Equal(expectedVolvoStatus, result.TaskStatusStates[0].VolvoRoTaskStatus);
        }


        [Fact]
        public void TaskStatus_ShouldReturnInProgress_WhenTaskHasLaborEntries()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.OPEN);
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = RepairOrderTaskStatus.OPEN,
                LaborEntries = new List<Labor> { new Labor() }
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Single(result.TaskStatusStates);
            Assert.Equal(VolvoRepairOrderTaskStatus.IN_PROGRESS, result.TaskStatusStates[0].VolvoRoTaskStatus);
        }

        [Fact]
        public void TaskStatus_ShouldReturnNotStarted_WhenTaskHasNoLaborEntries()
        {
            // Arrange
            var repairOrder = CreateRepairOrderSnapshot(RepairOrderStatus.OPEN);
            repairOrder.Tasks.Add(new RepairOrderTask
            {
                TaskNumber = 1,
                RepairTaskStatus = RepairOrderTaskStatus.OPEN
            });
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).GetVolvoRepairOrderStatus(repairOrder, lastRoWithState);

            // Assert
            Assert.Single(result.TaskStatusStates);
            Assert.Equal(VolvoRepairOrderTaskStatus.WORK_NOT_STARTED, result.TaskStatusStates[0].VolvoRoTaskStatus);
        }


        private RepairOrderSnapshot CreateRepairOrderSnapshot(string status)
        {
            return new RepairOrderSnapshot
            {
                RepairOrderStatus = status,
                SubStatus = string.Empty,
                CustomerContactedStatus = string.Empty,
                Tasks = new List<RepairOrderTask>()
            };
        }

        private VolvoRepairOrderState CreateVolvoRepairOrderWithState(string status, bool hasSentVehicleArrived, bool roHasBeenClosed)
        {
            return new VolvoRepairOrderState
            {
                HasSentVehicleArrival = hasSentVehicleArrived,
                RoHasBeenClosed = roHasBeenClosed,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = status,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
        }

    }
}