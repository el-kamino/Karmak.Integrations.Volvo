using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.Gen.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace Karmak.Integrations.Volvo.React.Tests.Mappers.RepairOrder._6_0_0
{
    public class VolvoRepairOrderBuilderTests
    {
        private readonly VolvoSettings _volvoSettings;
        private readonly RepairOrderSnapshot _repairOrder;
        private readonly VolvoRepairOrderState _roState;
        private readonly VolvoRepairOrderState _roClosedState;

        public VolvoRepairOrderBuilderTests()
        {
            _volvoSettings = new VolvoSettings
            {
                InterfaceOptions = new InterfaceOptions
                {
                    PaCode = "12345",
                    FranchiseCode = "F",
                    OemUserMappings = new Dictionary<string, string>()
                },
                RegionSettings = new RegionSettings
                {
                    CountryCode = "US",
                    LanguageCode = "en",
                    CurrencyCode = "USD"
                }
            };

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
                    CustomerKey = "CUST001"
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

            _roState = new VolvoRepairOrderState
            {
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.IN_PROGRESS
                },
                OriginalRoOpenDate = DateTime.UtcNow.AddDays(-1)
            };

            _roClosedState = new VolvoRepairOrderState
            {
                RepairOrderStatusState = new VolvoRepairOrderStatusState
                {
                    VolvoRoStatus = VolvoRepairOrderStatus.CLOSED
                },
                OriginalRoOpenDate = DateTime.UtcNow.AddDays(-1)
            };
        }

        [Fact]
        public void BuildSerializedPayload_ReturnsValidJsonString()
        {
            // Arrange
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);
            Assert.NotNull(payload);
        }

        [Fact]
        public void BuildSerializedPayload_ContainsCorrectHeader()
        {
            // Arrange
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.NotNull(payload.header);
            Assert.Equal(DspConstants.Identifier, payload.header.partyId);
            Assert.Equal("USA12345", payload.header.siteCode);
            Assert.Equal("USA", payload.header.dealerCountryCode);
            Assert.Equal("12345", payload.header.dealerNumberId);
            Assert.Equal(1, payload.header.roTotal);
        }

        [Fact]
        public void BuildSerializedPayload_ContainsCorrectSender_WhenHistorical()
        {
            // Arrange
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, true);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.NotNull(payload.sender);
            Assert.Equal(TransmissionType.Historical, payload.sender.taskId);
            Assert.Equal("en", payload.sender.languageCode);
            Assert.Equal("USD", payload.sender.currencyId);
        }

        [Fact]
        public void BuildSerializedPayload_ContainsCorrectSender_WhenNotHistorical()
        {
            // Arrange
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.NotNull(payload.sender);
            Assert.Equal(TransmissionType.New, payload.sender.taskId);
        }

        [Fact]
        public void BuildSerializedPayload_ContainsCorrectPayload()
        {
            // Arrange
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Equal("RO123", payload.payload[0].documentId);
            Assert.Equal("PO456", payload.payload[0].customerPurchaseOrderNumber);
            Assert.Equal(VolvoRepairOrderStatus.IN_PROGRESS, payload.payload[0].statusText);
            Assert.Equal("KEY123", payload.payload[0].vehicleHatNumber);
        }

        [Fact]
        public void BuildSerializedPayload_ContainsCorrectVehicle()
        {
            // Arrange
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.NotNull(payload.payload[0].vehicle);
            Assert.Equal("1HGBH41JXMN109186", payload.payload[0].vehicle.vehicleId);
            Assert.Equal(2023, payload.payload[0].vehicle.modelYear);
            Assert.Equal("Volvo", payload.payload[0].vehicle.makeString);
            Assert.Equal("F-150", payload.payload[0].vehicle.model);
            Assert.Equal("UNIT001", payload.payload[0].vehicle.fleetVehicleId);
        }

        [Fact]
        public void BuildSerializedPayload_PadsVinTo17Characters()
        {
            // Arrange
            _repairOrder.Vehicle.VIN = "SHORT";
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Equal(17, payload.payload[0].vehicle.vehicleId.Length);
            Assert.Equal("000000000000SHORT", payload.payload[0].vehicle.vehicleId);
        }

        [Fact]
        public void BuildSerializedPayload_UsesUnknownVin_WhenVinIsEmpty()
        {
            // Arrange
            _repairOrder.Vehicle.VIN = "";
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Equal("0000000000UNKNOWN", payload.payload[0].vehicle.vehicleId);
        }

        [Fact]
        public void BuildSerializedPayload_UsesInvoiceDate_WhenGreaterThanOpenDate()
        {
            // Arrange
            _repairOrder.OpenDate = DateTime.Parse("1/1/2026 10:00");
            _repairOrder.InvoiceDate = DateTime.Parse("1/1/2026 10:20");

            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roClosedState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Equal("2026-01-01T10:20:00-06:00", payload.payload[0].repairOrderCompletedDateTime);
        }

        [Fact]
        public void BuildSerializedPayload_UsesInvoiceDate_WhenGreaterThanOpenDate_ButCompletedDateLessThanOpneDate()
        {
            // Arrange
            _repairOrder.OpenDate = DateTime.Parse("1/1/2026 10:00");
            _repairOrder.CompletionDate = DateTime.Parse("1/1/2026 9:50");
            _repairOrder.InvoiceDate = DateTime.Parse("1/1/2026 10:10");

            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roClosedState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Equal("2026-01-01T10:10:00-06:00", payload.payload[0].repairOrderCompletedDateTime);
        }


        [Fact]
        public void BuildSerializedPayload_ContainsCorrectOdometer()
        {
            // Arrange
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.NotNull(payload.payload[0].inDistanceMeasure);
            Assert.Equal(50000, payload.payload[0].inDistanceMeasure.value);
            Assert.Equal(VolvoMeterType.Mile, payload.payload[0].inDistanceMeasure.unit);
        }

        [Fact]
        public void BuildSerializedPayload_CapsOdometerAtMaxValue()
        {
            // Arrange
            _repairOrder.MeterReading = 9999999;
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.Equal(999999, payload.payload[0].inDistanceMeasure.value);
        }


        [Fact]
        public void BuildSerializedPayload_OdometerTrimsDecimals()
        {
            // Arrange
            _repairOrder.MeterReading = (decimal?)123.45;
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.Equal(123, payload.payload[0].inDistanceMeasure.value);
        }

        [Fact]
        public void BuildSerializedPayload_UsesDefaultOdometer_WhenNull()
        {
            // Arrange
            _repairOrder.MeterReading = null;
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Equal(1, payload.payload[0].inDistanceMeasure.value);
        }

        [Fact]
        public void BuildPhantomVehicleArrival_SetsCorrectStatus()
        {
            // Arrange
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);
            var realPayload = builder.BuildSerializedPayload();

            // Act
            var phantomPayload = builder.BuildPhantomVehicleArrival(realPayload);
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(phantomPayload);

            // Assert
            Assert.NotNull(payload);
            Assert.Equal(VolvoRepairOrderStatus.ARRIVED, payload.payload[0].statusText);
            Assert.Null(payload.payload[0].job);
        }

        [Fact]
        public void BuildPhantomTechnicianAllocated_SetsCorrectStatusAndTaskStatus()
        {
            // Arrange
            _repairOrder.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    LaborEntries = new List<Labor>
                    {
                        new Labor
                        {
                            TechnicianUsername = "tech1",
                            DateTimeIn = DateTime.UtcNow.AddHours(-2),
                            DateTimeOut = DateTime.UtcNow.AddHours(-1)
                        }
                    }
                }
            };

            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);
            var realPayload = builder.BuildSerializedPayload();

            // Act
            var phantomPayload = builder.BuildPhantomTechnicianAllocated(realPayload);
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(phantomPayload);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Equal(VolvoRepairOrderStatus.TECH_ALLOCATED, payload.payload[0].statusText);
        }

        [Fact]
        public void BuildSerializedPayload_IncludesServiceAdvisor_WhenProvided()
        {
            // Arrange
            _repairOrder.ServiceWriterName = "John Doe";
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.NotNull(payload.payload[0].serviceAdvisorParty);
            Assert.Equal("John Doe", payload.payload[0].serviceAdvisorParty[0].id);
            Assert.Equal(CustomerTypeCodes.PartyIdentifierLocal, payload.payload[0].serviceAdvisorParty[0].type);
        }

        [Fact]
        public void BuildSerializedPayload_OmitsServiceAdvisor_WhenNull()
        {
            // Arrange
            _repairOrder.ServiceWriterName = null;
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Null(payload.payload[0].serviceAdvisorParty);
        }

        [Fact]
        public void BuildSerializedPayload_MapsOemUserNames()
        {
            // Arrange
            _repairOrder.ServiceWriterName = "J.Doe";
            _volvoSettings.InterfaceOptions.OemUserMappings = new Dictionary<string, string>
            {
                { "JDoe", "JDOE001" }
            };
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.NotNull(payload.payload[0].serviceAdvisorParty);
            Assert.NotEmpty(payload.payload[0].serviceAdvisorParty);
            Assert.Equal("JDOE001", payload.payload[0].serviceAdvisorParty[0].id);
        }

        [Fact]
        public void BuildSerializedPayload_IncludesSecondaryReferenceNumber_WhenProvided()
        {
            // Arrange
            _repairOrder.OriginalRepairOrderNumber = "ORIG123";
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Equal("ORIG123", payload.payload[0].secondaryReferenceNumberString);
        }

        [Fact]
        public void BuildSerializedPayload_OmitsSecondaryReferenceNumber_WhenNull()
        {
            // Arrange
            _repairOrder.OriginalRepairOrderNumber = null;
            var builder = new VolvoRepairOrderBuilder(_volvoSettings, _repairOrder, _roState, false);

            // Act
            var result = builder.BuildSerializedPayload();
            var payload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(result);

            // Assert
            Assert.NotNull(payload);
            Assert.NotNull(payload.payload);
            Assert.NotEmpty(payload.payload);
            Assert.Null(payload.payload[0].secondaryReferenceNumberString);
        }

        [Fact]
        public void WafSanitize_IsCalledOnAllRequiredStrings()
        {
            // Arrange
            var serviceWriterName = "Test<Script>Writer";
            var oemUserMappings = new Dictionary<string, string>();

            var vehicle = new Vehicle
            {
                Make = "Test<<Script>>",
                Model = "Test<javascript:",
                UnitNumber = "Test<iframe>",
                VIN = "12345678901234567"
            };

            // Act - BuildServiceAdvisorParty
            var serviceAdvisorResult = InvokeBuildServiceAdvisorParty(serviceWriterName, oemUserMappings);

            // Act - BuildVehicle
            var vehicleResult = InvokeBuildVehicle(vehicle);

            // Assert - Verify sanitization occurred by checking special characters are removed/escaped
            Assert.NotNull(serviceAdvisorResult);
            Assert.Single(serviceAdvisorResult);
            Assert.DoesNotContain("<", serviceAdvisorResult[0].id);
            Assert.DoesNotContain(">", serviceAdvisorResult[0].id);

            Assert.NotNull(vehicleResult);
            Assert.DoesNotContain("<", vehicleResult.makeString);
            Assert.DoesNotContain(">", vehicleResult.makeString);
            Assert.DoesNotContain("<", vehicleResult.model);
            Assert.DoesNotContain(">", vehicleResult.model);
            Assert.DoesNotContain("<", vehicleResult.fleetVehicleId);
            Assert.DoesNotContain(">", vehicleResult.fleetVehicleId);
        }

        private static List<VolvoPartyIdentifier>? InvokeBuildServiceAdvisorParty(string serviceWriterName, IDictionary<string, string> oemUserMappings)
        {
            var method = typeof(VolvoRepairOrderBuilder).GetMethod("BuildServiceAdvisorParty",
                BindingFlags.NonPublic | BindingFlags.Static);

            var result = method?.Invoke(null, new object[] { serviceWriterName, oemUserMappings }) as List<VolvoPartyIdentifier>;
            return result;
        }

        private static VolvoVehicle? InvokeBuildVehicle(Vehicle vehicle)
        {
            var method = typeof(VolvoRepairOrderBuilder).GetMethod("BuildVehicle",
                BindingFlags.NonPublic | BindingFlags.Static);

            return method?.Invoke(null, new object[] { vehicle }) as VolvoVehicle;
        }
    }
}