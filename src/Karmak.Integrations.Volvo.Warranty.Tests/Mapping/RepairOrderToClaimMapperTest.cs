using AutoBogus;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using RepairOrderAddress = Karmak.Integrations.Volvo.React.Contracts.Common.Address;
using RepairOrderLabor = Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data.Labor;
using RepairOrderLaborOperation = Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data.LaborOperation;
using RepairOrderMiscCharge = Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data.MiscCharge;
using RepairOrderPart = Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data.Part;
using RepairOrderCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;
using RepairOrderVehicle = Karmak.Integrations.Volvo.React.Contracts.Common.Vehicle;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Mapping
{
    public class RepairOrderToClaimMapperTest
    {
        private readonly RepairOrderToClaimMapper _mapper = new RepairOrderToClaimMapper();

        private static RepairOrderSnapshot CreateFakeRepairOrderSnapshot() =>
            AutoFaker.Generate<RepairOrderSnapshot>(builder =>
                builder.WithRecursiveDepth(3));

        private static VolvoSettings CreateFakeSettings(
            string currencyCode = "USD",
            int[] subletLaborCharges = null,
            IDictionary<string, string> oemUserMappings = null)
        {
            return new VolvoSettings
            {
                RegionSettings = new RegionSettings
                {
                    CurrencyCode = currencyCode
                },
                InterfaceOptions = new InterfaceOptions
                {
                    SubletLaborCharges = subletLaborCharges ?? Array.Empty<int>(),
                    OemUserMappings = oemUserMappings ?? new Dictionary<string, string>()
                }
            };
        }

        [Fact]
        public void WhenMapping_It_MapsClaimTypeToVehicleCoverage()
        {
            var source = CreateFakeRepairOrderSnapshot();

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal(ClaimTypes.VehicleCoverages, result.Type);
        }

        [Fact]
        public void WhenMapping_It_SetsOemToVolvo()
        {
            var source = CreateFakeRepairOrderSnapshot();

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal("Volvo", result.Oem.Name);
        }

        [Fact]
        public void WhenMappingAdvisor_It_UsesMappingToOemId()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.ServiceWriterName = "john.doe";
            var oemMappings = new Dictionary<string, string> { { "johndoe", "OEM123" } };
            var settings = CreateFakeSettings(oemUserMappings: oemMappings);

            var result = _mapper.Map(source, settings, null);

            Assert.Equal("OEM123", result.RepairOrder.Advisor.Identifier);
        }

        [Fact]
        public void WhenMapping_MapsInServiceDate()
        {
            var source = CreateFakeRepairOrderSnapshot();
            var expectedDate = new DateTime(2025, 6, 15);
            source.InServiceDate = expectedDate;

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal(expectedDate, result.Unit.Vehicle!.InServiceDate);
        }

        [Fact]
        public void WhenMapping_MapsMeterReadingType()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.MeterType = "Miles";
            source.MeterReading = 50000m;
            source.Vehicle = new RepairOrderVehicle { VIN = "VIN123" };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            var odometerReading = result.Unit.MeterReadings.First(r => r.Type == MeterReadingType.Odometer);
            Assert.Equal(UnitOfMeasureType.Miles, odometerReading.ReadingIn.UnitOfMeasure);
            Assert.Equal(50000m, odometerReading.ReadingIn.Value);
        }

        [Fact]
        public void WhenMappingAdvisorWithoutOemId_It_UsesTheUsername()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.ServiceWriterName = "john.doe";
            var settings = CreateFakeSettings(oemUserMappings: new Dictionary<string, string>());

            var result = _mapper.Map(source, settings, null);

            Assert.Equal("john.doe", result.RepairOrder.Advisor.Identifier);
            Assert.Equal("john.doe", result.RepairOrder.Advisor.Username);
        }

        [Fact]
        public void WhenMapping_It_MapsEachTaskToAJob()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask { TaskNumber = 1, Parts = new List<RepairOrderPart>(), MiscCharges = new List<RepairOrderMiscCharge>(), LaborEntries = new List<RepairOrderLabor>() },
                new RepairOrderTask { TaskNumber = 2, Parts = new List<RepairOrderPart>(), MiscCharges = new List<RepairOrderMiscCharge>(), LaborEntries = new List<RepairOrderLabor>() },
                new RepairOrderTask { TaskNumber = 3, Parts = new List<RepairOrderPart>(), MiscCharges = new List<RepairOrderMiscCharge>(), LaborEntries = new List<RepairOrderLabor>() }
            };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal(3, result.RepairOrder.Jobs!.Count());
        }

        [Fact]
        public void WhenMapping_It_MapsRepairOrderNumberToEachClaimIdentifier()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.RepairOrderNumber = "RO-12345";

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal("RO-12345", result.Identifier);
        }

        [Fact]
        public void WhenMapping_It_DoesNotMapDealer()
        {
            var source = CreateFakeRepairOrderSnapshot();

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Null(result.Dealer);
        }

        [Fact]
        public void WhenMapping_It_MapsOwningCustomerToCustomer()
        {
            var source = CreateFakeRepairOrderSnapshot();
            var contact = new Contact
            {
                FirstName = "John",
                MiddleName = "M",
                LastName = "Smith",
                Email = "john@example.com",
                Phones = new List<Phone>
                {
                    new Phone { Type = PhoneType.WORK, Number = "555-1234" },
                    new Phone { Type = PhoneType.CELL, Number = "555-5678" }
                }
            };
            source.OwningCustomer = new RepairOrderCustomer
            {
                CustomerKey = "CUST001",
                CompanyName = "Acme Corp",
                Contacts = new List<Contact> { contact }
            };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal("CUST001", result.Customer.Identifier);
            Assert.Equal("Acme Corp", result.Customer.CompanyName);
            Assert.Equal("John", result.Customer.FirstName);
            Assert.Equal("M", result.Customer.MiddleName);
            Assert.Equal("Smith", result.Customer.LastName);
            Assert.Equal("john@example.com", result.Customer.EmailAddress);
            Assert.Equal("555-1234", result.Customer.WorkPhoneNumber);
            Assert.Equal("555-5678", result.Customer.CellPhoneNumber);
        }

        [Fact]
        public void WhenMapping_It_MapsOwningShippingAddressToCustomerAddressInfo()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.OwningCustomer = new RepairOrderCustomer
            {
                Contacts = new List<Contact> { new Contact() }
            };
            source.Addresses = new List<RepairOrderAddress>
            {
                new RepairOrderAddress
                {
                    AddressType = AddressType.SHIP_TO,
                    EntityType = ConstantSettings.OwningCustomerAddressEntityType,
                    Street1 = "123 Main St",
                    Street2 = "Suite 100",
                    City = "Springfield",
                    Region = "IL",
                    County = "Sangamon",
                    PostalCode = "62701",
                    CountryCode = "US"
                }
            };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.NotNull(result.Customer.PhysicalAddress);
            Assert.Equal("123 Main St", result.Customer.PhysicalAddress.StreetAddress);
            Assert.Equal("Suite 100", result.Customer.PhysicalAddress.SecondaryAddress);
            Assert.Equal("Springfield", result.Customer.PhysicalAddress.City);
            Assert.Equal("IL", result.Customer.PhysicalAddress.Region);
            Assert.Equal("Sangamon", result.Customer.PhysicalAddress.County);
            Assert.Equal("62701", result.Customer.PhysicalAddress.PostalCode);
            Assert.Equal(CountryCode.US, result.Customer.PhysicalAddress.Country);
        }

        [Fact]
        public void WhenOwningShippingAddressIsNotFound_It_MapsCustomerAddressToNull()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.OwningCustomer = new RepairOrderCustomer
            {
                Contacts = new List<Contact> { new Contact() }
            };
            source.Addresses = new List<RepairOrderAddress>
            {
                new RepairOrderAddress
                {
                    AddressType = AddressType.BILL_TO,
                    EntityType = "SomeOtherEntity"
                }
            };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Null(result.Customer.PhysicalAddress);
        }

        [Fact]
        public void WhenUnsupportedCountry_It_DefaultsToUS()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.OwningCustomer = new RepairOrderCustomer
            {
                Contacts = new List<Contact> { new Contact() }
            };
            source.Addresses = new List<RepairOrderAddress>
            {
                new RepairOrderAddress
                {
                    AddressType = AddressType.SHIP_TO,
                    EntityType = ConstantSettings.OwningCustomerAddressEntityType,
                    CountryCode = "XX"
                }
            };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal(CountryCode.US, result.Customer.PhysicalAddress!.Country);
        }

        [Fact]
        public void WhenMapping_It_MapsDriver()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Driver = new Contact
            {
                FirstName = "Jane",
                MiddleName = "Marie",
                LastName = "Doe",
                LicenseNumber = "DL123456"
            };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal("DL123456", result.Driver!.Identifier);
            Assert.Equal("Jane M Doe", result.Driver.FullName);
        }

        [Fact]
        public void WhenMappingDriverWithoutMiddleName_It_MapsToDriverFullName()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Driver = new Contact
            {
                FirstName = "Jane",
                MiddleName = null,
                LastName = "Doe",
                LicenseNumber = "DL123456"
            };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal("Jane Doe", result.Driver!.FullName);
        }

        [Fact]
        public void WhenMapping_It_MapsToTheRepairOrder()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.RepairOrderNumber = "RO-100";
            source.OriginalRepairOrderNumber = "RO-099";
            source.OpenDate = new DateTime(2025, 1, 10, 8, 30, 0);
            source.CompletionDate = new DateTime(2025, 1, 11, 16, 0, 0);
            source.InvoiceDate = new DateTime(2025, 1, 12, 9, 0, 0);
            source.InvoiceIdentifier = "INV-001";
            source.Tasks = new List<RepairOrderTask>();

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal("RO-099", result.RepairOrder.Identifier);
            Assert.Equal("RO-100", result.RepairOrder.SecondaryIdentifier);
            Assert.Equal("INV-001", result.RepairOrder.InvoiceIdentifier);
            Assert.Equal(new DateTime(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc), result.RepairOrder.OpenedDate);
            Assert.Equal(new DateTime(2025, 1, 11, 12, 0, 0, DateTimeKind.Utc), result.RepairOrder.CompletedDate);
            Assert.Equal(new DateTime(2025, 1, 12, 12, 0, 0, DateTimeKind.Utc), result.RepairOrder.InvoicedDate);
        }

        [Fact]
        public void WhenMissingOriginalRepairOrderNumber_It_MapsRepairOrderNumberToBothRepairOrderIdentifiers()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.RepairOrderNumber = "RO-100";
            source.OriginalRepairOrderNumber = null;
            source.Tasks = new List<RepairOrderTask>();

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal("RO-100", result.RepairOrder.Identifier);
            Assert.Equal("RO-100", result.RepairOrder.SecondaryIdentifier);
        }

        [Fact]
        public void WhenMapping_It_MapsToTheJob()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 42,
                    ComplaintNotes = "Engine noise",
                    TechnicianNotes = "Replaced belt",
                    InternalDealerNotes = "Warranty covered",
                    Parts = new List<RepairOrderPart>(),
                    MiscCharges = new List<RepairOrderMiscCharge>(),
                    LaborEntries = new List<RepairOrderLabor>()
                }
            };

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            var job = result.RepairOrder.Jobs!.First();
            Assert.Equal("42", job.Identifier);
            Assert.Equal("Engine noise", job.CustomerNotes);
            Assert.Equal("Replaced belt", job.TechnicianNotes);
            Assert.Equal("Warranty covered", job.InternalDealerNotes);
        }

        [Fact]
        public void WhenMapping_It_MapsToEachJobLaborExpense()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    LaborRate = 100m,
                    Parts = new List<RepairOrderPart>(),
                    MiscCharges = new List<RepairOrderMiscCharge>(),
                    LaborEntries = new List<RepairOrderLabor>
                    {
                        new RepairOrderLabor
                        {
                            UnitPrice = 120m,
                            TechnicianUsername = "tech1",
                            TechnicianNumber = 101
                        }
                    },
                    LaborOperations = new List<RepairOrderLaborOperation>
                    {
                        new RepairOrderLaborOperation { Code = "LAB01", Description = "Diag", Hours = 1.5m },
                        new RepairOrderLaborOperation { Code = "LAB02", Description = "Repair", Hours = 2.0m }
                    }
                }
            };
            var settings = CreateFakeSettings();

            var result = _mapper.Map(source, settings, null);

            var job = result.RepairOrder.Jobs!.First();
            Assert.Equal(2, job.LaborExpenses.Count);
            Assert.Equal("LAB01", job.LaborExpenses[0].Identifier);
            Assert.Equal("Diag", job.LaborExpenses[0].Description);
            Assert.Equal(1.5m, job.LaborExpenses[0].Quantity.Value);
            Assert.Equal(UnitOfMeasureType.Hours, job.LaborExpenses[0].Quantity.Type);
            Assert.Equal(120m, job.LaborExpenses[0].UnitPrice.Value);
        }

        [Fact]
        public void WhenMappingTechnician_It_UsesTechnicianUsernameMappingToOemId()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    Parts = new List<RepairOrderPart>(),
                    MiscCharges = new List<RepairOrderMiscCharge>(),
                    LaborEntries = new List<RepairOrderLabor>
                    {
                        new RepairOrderLabor
                        {
                            TechnicianUsername = "tech1",
                            TechnicianNumber = 101,
                            UnitPrice = 50m
                        }
                    },
                    LaborOperations = new List<RepairOrderLaborOperation>
                    {
                        new RepairOrderLaborOperation { Code = "LAB01", Hours = 1m }
                    }
                }
            };
            var oemMappings = new Dictionary<string, string> { { "tech1", "OEMTECH01" } };
            var settings = CreateFakeSettings(oemUserMappings: oemMappings);

            var result = _mapper.Map(source, settings, null);

            var technician = result.RepairOrder.Jobs!.First().LaborExpenses[0].Technician;
            Assert.Equal("OEMTECH01", technician.Identifier);
            Assert.Equal("tech1", technician.Username);
        }

        [Fact]
        public void WhenMapping_It_MapsToEachPartExpense()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    LaborEntries = new List<RepairOrderLabor>(),
                    MiscCharges = new List<RepairOrderMiscCharge>(),
                    Parts = new List<RepairOrderPart>
                    {
                        new RepairOrderPart
                        {
                            PartNumber = "AB1234CD5678EF90",
                            Description = "Brake Pad",
                            UnitPrice = 45.00m,
                            UnitCost = 30.00m,
                            Quantity = 2m,
                            PartType = "Regular"
                        },
                        new RepairOrderPart
                        {
                            PartNumber = "XY9876ZW5432QR21",
                            Description = "Rotor",
                            UnitPrice = 80.00m,
                            UnitCost = 60.00m,
                            Quantity = 1m,
                            PartType = "Regular"
                        }
                    }
                }
            };
            var settings = CreateFakeSettings();

            var result = _mapper.Map(source, settings, null);

            var job = result.RepairOrder.Jobs!.First();
            Assert.Equal(2, job.PartExpenses.Count);

            var firstPart = job.PartExpenses[0];
            Assert.Equal("AB1234CD5678EF90", firstPart.Identifier);
            Assert.Equal("Brake Pad", firstPart.Description);
            Assert.Equal(45.00m, firstPart.UnitPrice.Value);
            Assert.Equal(30.00m, firstPart.UnitCost.Value);
            Assert.Equal(2m, firstPart.Quantity.Value);
            Assert.Equal(UnitOfMeasureType.Count, firstPart.Quantity.Type);
        }

        [Fact]
        public void WhenMappingParts_It_MapsMoneyUsingDealerProvidedCurrency()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    LaborEntries = new List<RepairOrderLabor>(),
                    MiscCharges = new List<RepairOrderMiscCharge>(),
                    Parts = new List<RepairOrderPart>
                    {
                        new RepairOrderPart
                        {
                            PartNumber = "AB1234CD5678EF90",
                            UnitPrice = 10m,
                            UnitCost = 5m,
                            Quantity = 1m,
                            PartType = "Regular"
                        }
                    }
                }
            };
            var settings = CreateFakeSettings(currencyCode: "CAD");

            var result = _mapper.Map(source, settings, null);

            var partExpense = result.RepairOrder.Jobs!.First().PartExpenses[0];
            Assert.Equal(CurrencyCode.CAD, partExpense.UnitPrice.Currency);
            Assert.Equal(CurrencyCode.CAD, partExpense.UnitCost.Currency);
        }

        [Fact]
        public void WhenMapping_It_MapsToEachMiscellaneousExpense()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    LaborEntries = new List<RepairOrderLabor>(),
                    Parts = new List<RepairOrderPart>(),
                    MiscCharges = new List<RepairOrderMiscCharge>
                    {
                        new RepairOrderMiscCharge
                        {
                            MiscellaneousChargeID = 100,
                            Name = "Towing",
                            Description = "Tow service",
                            Quantity = 1m,
                            UnitPrice = 75m,
                            InvoiceIdentifier = "TOW-001"
                        },
                        new RepairOrderMiscCharge
                        {
                            MiscellaneousChargeID = 200,
                            Name = "Storage",
                            Description = "Overnight storage",
                            Quantity = 3m,
                            UnitPrice = 25m
                        }
                    }
                }
            };
            var settings = CreateFakeSettings();

            var result = _mapper.Map(source, settings, null);

            var job = result.RepairOrder.Jobs!.First();
            Assert.Equal(2, job.MiscellaneousExpenses.Count);
            Assert.Equal("Towing", job.MiscellaneousExpenses[0].Identifier);
            Assert.Equal("Tow service", job.MiscellaneousExpenses[0].Description);
            Assert.Equal(1m, job.MiscellaneousExpenses[0].Quantity.Value);
            Assert.Equal(UnitOfMeasureType.Days, job.MiscellaneousExpenses[0].Quantity.Type);
            Assert.Equal(75m, job.MiscellaneousExpenses[0].UnitPrice.Value);
            Assert.Equal("TOW-001", job.MiscellaneousExpenses[0].ThirdPartyInvoiceIdentifier);
        }

        [Fact]
        public void WhenMappingMiscellaneousCharges_It_ExcludesSubletChargesFromMiscellaneousExpenses()
        {
            var subletChargeId = 999;
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    LaborEntries = new List<RepairOrderLabor>(),
                    Parts = new List<RepairOrderPart>(),
                    MiscCharges = new List<RepairOrderMiscCharge>
                    {
                        new RepairOrderMiscCharge
                        {
                            MiscellaneousChargeID = subletChargeId,
                            Name = "Sublet Labor",
                            Description = "Sublet labor charge",
                            Quantity = 2m,
                            UnitPrice = 50m
                        },
                        new RepairOrderMiscCharge
                        {
                            MiscellaneousChargeID = 100,
                            Name = "Regular Misc",
                            Description = "Regular charge",
                            Quantity = 1m,
                            UnitPrice = 30m
                        }
                    }
                }
            };
            var settings = CreateFakeSettings(subletLaborCharges: new[] { subletChargeId });

            var result = _mapper.Map(source, settings, null);

            var job = result.RepairOrder.Jobs!.First();
            Assert.Single(job.MiscellaneousExpenses);
            Assert.Equal("Regular Misc", job.MiscellaneousExpenses[0].Identifier);
        }

        [Fact]
        public void WhenMappingMiscellaneousCharges_It_IncludesSubletChargesAsALaborExpense()
        {
            var subletChargeId = 999;
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    LaborEntries = new List<RepairOrderLabor>(),
                    Parts = new List<RepairOrderPart>(),
                    LaborOperations = new List<RepairOrderLaborOperation>(),
                    MiscCharges = new List<RepairOrderMiscCharge>
                    {
                        new RepairOrderMiscCharge
                        {
                            MiscellaneousChargeID = subletChargeId,
                            Description = "Sublet labor charge",
                            Quantity = 2m,
                            UnitPrice = 50m,
                            InvoiceIdentifier = "SUB-001"
                        }
                    }
                }
            };
            var settings = CreateFakeSettings(subletLaborCharges: new[] { subletChargeId });

            var result = _mapper.Map(source, settings, null);

            var job = result.RepairOrder.Jobs!.First();
            Assert.Single(job.LaborExpenses);
            var subletLabor = job.LaborExpenses[0];
            Assert.True(subletLabor.IsSublet);
            Assert.Equal("Sublet labor charge", subletLabor.Description);
            Assert.Equal(2m, subletLabor.Quantity.Value);
            Assert.Equal(UnitOfMeasureType.Hours, subletLabor.Quantity.Type);
            Assert.Equal(50m, subletLabor.UnitPrice.Value);
            Assert.Null(subletLabor.Technician);
            Assert.Equal("SUB-001", subletLabor.ThirdPartyInvoiceIdentifier);
        }

        [Fact]
        public void WhenMapping_It_MapsToUnit()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Vehicle = new RepairOrderVehicle
            {
                UnitNumber = "UNIT-001",
                UnitInventoryIdentifier = "INV-UNIT-001",
                VIN = "1HGCM82633A004352"
            };
            source.MeterType = "Miles";
            source.MeterReading = 75000m;

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal("UNIT-001", result.Unit.Identifier);
            Assert.NotNull(result.Unit.MeterReadings);
            Assert.Contains(result.Unit.MeterReadings, r => r.Type == MeterReadingType.Odometer);

            var externalId = result.Unit.ExternalIdentifiers.First();
            Assert.Equal("INV-UNIT-001", externalId.ID);
            Assert.Equal("FUSION", externalId.ExternalSourceType);
        }

        [Fact]
        public void WhenMapping_It_MapsToVehicle()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Vehicle = new RepairOrderVehicle
            {
                VIN = "1HGCM82633A004352",
                Make = "Volvo",
                Model = "F-150",
                Year = "2025"
            };
            source.InServiceDate = new DateTime(2024, 3, 15);

            var result = _mapper.Map(source, CreateFakeSettings(), "TX");

            Assert.Equal("1HGCM82633A004352", result.Unit.Vehicle!.Identifier);
            Assert.Equal("Volvo", result.Unit.Vehicle.Make);
            Assert.Equal("F-150", result.Unit.Vehicle.Model);
            Assert.Equal("2025", result.Unit.Vehicle.Year);
            Assert.Equal(new DateTime(2024, 3, 15), result.Unit.Vehicle.InServiceDate);
        }

        [Fact]
        public void WhenMapping_It_MapsExternalIdentifiers()
        {
            var source = CreateFakeRepairOrderSnapshot();
            var externalIds = new List<React.Contracts.ExternalIdentifier>
            {
                new React.Contracts.ExternalIdentifier { ID = "EXT1", ExternalSourceType = "DMS" }
            };
            var origExternalIds = new List<React.Contracts.ExternalIdentifier>
            {
                new React.Contracts.ExternalIdentifier { ID = "ORIG1", ExternalSourceType = "DMS" }
            };
            source.ExternalIdentifiers = externalIds;
            source.OriginalRepairOrderExternalIdentifiers = origExternalIds;
            source.Tasks = new List<RepairOrderTask>();

            var result = _mapper.Map(source, CreateFakeSettings(), null);

            Assert.Equal(externalIds, result.RepairOrder.SecondaryExternalIdentifiers);
            Assert.Equal(origExternalIds, result.RepairOrder.ExternalIdentifiers);
        }

        [Fact]
        public void WhenMapping_It_ConcatsExpenses()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Tasks = new List<RepairOrderTask>
            {
                new RepairOrderTask
                {
                    TaskNumber = 1,
                    LaborEntries = new List<RepairOrderLabor>
                    {
                        new RepairOrderLabor { UnitPrice = 100m, TechnicianUsername = "tech1", TechnicianNumber = 1 }
                    },
                    Parts = new List<RepairOrderPart>
                    {
                        new RepairOrderPart
                        {
                            PartNumber = "AB1234CD5678EF90",
                            UnitPrice = 10m,
                            UnitCost = 5m,
                            Quantity = 1m,
                            PartType = "Regular"
                        }
                    },
                    LaborOperations = new List<RepairOrderLaborOperation>
                    {
                        new RepairOrderLaborOperation { Code = "LAB01", Hours = 1m }
                    },
                    MiscCharges = new List<RepairOrderMiscCharge>
                    {
                        new RepairOrderMiscCharge
                        {
                            MiscellaneousChargeID = 100,
                            Name = "Misc1",
                            Quantity = 1m,
                            UnitPrice = 20m
                        }
                    }
                }
            };
            var settings = CreateFakeSettings();

            var result = _mapper.Map(source, settings, null);

            var job = result.RepairOrder.Jobs!.First();
            var totalExpenses = job.PartExpenses.Count + job.LaborExpenses.Count + job.MiscellaneousExpenses.Count;
            Assert.Equal(totalExpenses, job.Expenses.Count);
        }

        [Fact]
        public void WhenMapping_It_MapsCustomerRegionToLicenseState()
        {
            var source = CreateFakeRepairOrderSnapshot();
            source.Vehicle = new RepairOrderVehicle
            {
                VIN = "1HGCM82633A004352"
            };
            var customerRegion = "CA";

            var result = _mapper.Map(source, CreateFakeSettings(), customerRegion);

            Assert.Equal("CA", result.Unit.Vehicle!.License.State);
        }
    }
}
