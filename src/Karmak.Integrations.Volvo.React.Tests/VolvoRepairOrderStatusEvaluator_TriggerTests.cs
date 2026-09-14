using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Karmak.Integrations.Volvo.React.Tests.RepairOrders.V6_0_0
{
    public class VolvoRepairOrderStatusEvaluator_TriggerTests
    {
        private readonly NullLogger<VolvoRepairOrderStatusEvaluator> _mockLogger;
        private readonly IKarmakBlobClient _mockBlobClient;
        private readonly Dictionary<string, string> _metaData;

        public VolvoRepairOrderStatusEvaluator_TriggerTests()
        {
            _mockLogger = new NullLogger<VolvoRepairOrderStatusEvaluator>();
            _mockBlobClient = Substitute.For<IKarmakBlobClient>();
            _metaData = new Dictionary<string, string>
            {
                { "RepairOrder.Number", "TEST-RO-123" }
            };
        }

        [Fact]
        public void RoTrigger_ShouldReturnFalse_WhenCurrentStatusIsUndefined()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.UNDEFINED,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).RepairOrderTriggersSend(
                currentState, lastRoWithState, true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RoTrigger_ShouldReturnTrue_WhenFirstTransmissionWithValidStatus()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = true,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.ARRIVED,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.UNDEFINED, true);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).RepairOrderTriggersSend(
                currentState, lastRoWithState, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RoTrigger_ShouldTriggerPhantomVEHICLE_ARRIVAL_WhenFirstTransmissionWithValidStatusIsNotVEHICLE_ARRIVAL()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = true,
                HasSentVehicleArrival = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.UNDEFINED, true);

            // Act
            var roEvaluator = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient);
            var shouldSend = roEvaluator.RepairOrderTriggersSend(currentState, lastRoWithState, false);
            var result = roEvaluator.ShouldSendPhantomVehicleArrival(currentState);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RoTrigger_ShouldNotTriggerPhantomVEHICLE_ARRIVAL_WhenFirstTransmissionIsVEHICLE_ARRIVAL()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = true,
                HasSentVehicleArrival = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.ARRIVED,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.UNDEFINED, true);

            // Act
            var roEvaluator = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient);
            var shouldSend = roEvaluator.RepairOrderTriggersSend(currentState, lastRoWithState, false);
            var result = roEvaluator.ShouldSendPhantomVehicleArrival(currentState);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RoTrigger_ShouldNotTriggerPhantomVEHICLE_ARRIVAL_WhenHasBeenSent()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = true,
                HasSentVehicleArrival = true,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.ARRIVED,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.UNDEFINED, true);

            // Act
            var roEvaluator = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient);
            var shouldSend = roEvaluator.RepairOrderTriggersSend(currentState, lastRoWithState, false);
            var result = roEvaluator.ShouldSendPhantomVehicleArrival(currentState);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RoTrigger_ShouldTriggerPhantomTECHNICIAN_ALLOCATED_WhenHasNotBeenSent()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = true,
                HasSentTechnicianAllocated = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.UNDEFINED, true);

            // Act
            var roEvaluator = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient);
            var result = roEvaluator.ShouldSendPhantomTechnicianAllocated(currentState);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RoTrigger_ShouldNotTriggerPhantomTECHNICIAN_ALLOCATED_WhenHasBeenSent()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = true,
                HasSentTechnicianAllocated = true,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.UNDEFINED, true);

            // Act
            var roEvaluator = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient);
            var result = roEvaluator.ShouldSendPhantomTechnicianAllocated(currentState);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RoTrigger_ShouldReturnTrue_WhenRepairOrderStatusChanges()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.ARRIVED, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).RepairOrderTriggersSend(
                currentState, lastRoWithState, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RoTrigger_ShouldReturnTrue_WhenTaskIsDeleted()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.IN_PROGRESS, false);
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 1,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
            });

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).RepairOrderTriggersSend(
                currentState, lastRoWithState, false);

            // Assert
            Assert.True(result);    
        }

        [Fact]
        public void RoTrigger_ShouldReturnTrue_WhenTaskStatusChanges()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>
                    {
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 1,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
                        },
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 2,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.WORK_NOT_STARTED
                        },
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 3,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.COMPLETED
                        },
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 4,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.ON_HOLD
                        }
                    }
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.IN_PROGRESS, false);
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 1,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
            });
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 2,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.WORK_NOT_STARTED
            });
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 3,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.COMPLETED
            });
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 4,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
            });

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).RepairOrderTriggersSend(
                currentState, lastRoWithState, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RoTrigger_ShouldReturnTrue_WhenTaskIsAdded()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>
                    {
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 1,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
                        }
                    }
                }
            };

            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.IN_PROGRESS, false);

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).RepairOrderTriggersSend(
                currentState, lastRoWithState, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RoTrigger_ShouldReturnFalse_WhenNoChanges()
        {
            // Arrange
            var currentState = new VolvoRepairOrderState
            {
                IsFirstTransmission = false,
                RepairOrderStatusState = new VolvoRepairOrderStatusState

                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>
                    {
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 1,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
                        },
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 2,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.WORK_NOT_STARTED
                        },
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 3,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.COMPLETED
                        },
                        new VolvoRepairOrderTaskStatusState
                        {
                            TaskNumber = 4,
                            VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.ON_HOLD
                        }
                    }
                }
            };
            var lastRoWithState = CreateVolvoRepairOrderWithState(VolvoRepairOrderStatus.IN_PROGRESS, false);
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 1,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.IN_PROGRESS
            });
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 2,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.WORK_NOT_STARTED
            });
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 3,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.COMPLETED
            });
            lastRoWithState.RepairOrderStatusState.TaskStatusStates.Add(new VolvoRepairOrderTaskStatusState
            {
                TaskNumber = 4,
                VolvoRoTaskStatus = VolvoRepairOrderTaskStatus.ON_HOLD
            });

            // Act
            var result = new VolvoRepairOrderStatusEvaluator(_mockLogger, _mockBlobClient).RepairOrderTriggersSend(
                currentState, lastRoWithState, false);

            // Assert
            Assert.False(result);
        }

        private VolvoRepairOrderState CreateVolvoRepairOrderWithState(string status, bool isFirstTransmission)
        {
            return new VolvoRepairOrderState
            {
                IsFirstTransmission = isFirstTransmission,
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = status,
                    TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>()
                }
            };
        }
    }
}