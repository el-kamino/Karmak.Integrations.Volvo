using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Mappers.RepairOrder._6_0_0;
using Karmak.Integrations.Volvo.React.RepairOrders;
using Xunit;

namespace Karmak.Integrations.Volvo.React.Tests.Mappers.RepairOrder._6_0_0
{
    public class VolvoRepairOrderLaborBuilderTests
    {
        private readonly RepairOrderSnapshot _repairOrder;
        private readonly VolvoSettings _volvoSettings;
        private readonly string _department = "Service";

        public VolvoRepairOrderLaborBuilderTests()
        {
            _repairOrder = new RepairOrderSnapshot
            {
                RepairOrderNumber = "RO123",
                CustomerPONumber = "PO456",
                RepairOrderStatus = "OPEN",
                KeyTag = "KEY123",
                DatabaseVersion = "1.0.0",
                Department = "Service",
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

            _volvoSettings = new VolvoSettings
            {
                InterfaceOptions = new InterfaceOptions
                {
                    SubletLaborCharges = new[] { 100, 101 },
                    SubletPartsCharges = new[] { 200, 201 },
                    WarrantyCustomers = new[] { "CUST123" },
                    WarrantyDeductibleCharges = new[] { 300, 301 },
                    EspCustomers = new[] { "CUST456" },
                    EspDeductibleCharges = new[] { 400, 401 }
                }
            };
        }

        [Fact]
        public void BuildServiceLabor_WithWarrantyTask_UsesTaskSRTID()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask(warranty: true, srtId: "SRT123", repairType: "RT456");

            var result = builder.BuildServiceLabor(task, OperationIds.Warranty, 42, 0, 1, 1);

            Assert.Equal("SRT123", result.laborOperationId);
        }

        [Fact]
        public void BuildServiceLabor_WithNonWarrantyTask_UsesRepairType()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask(warranty: false, srtId: "SRT123", repairType: "RT456");

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.Equal("RT456", result.laborOperationId);
        }

        [Fact]
        public void BuildServiceLabor_SetsLaborOperationDescription()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask(repairTypeDescription: "Test Repair");

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.Equal("Test Repair", result.laborOperationDescription);
        }

        [Fact]
        public void BuildServiceLabor_CalculatesLaborActualHours()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.LaborEntries.Add(new Labor { TotalHours = 2.5m, ExtendedPrice = 250m });
            task.LaborEntries.Add(new Labor { TotalHours = 1.5m, ExtendedPrice = 150m });

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.NotNull(result.laborActualHoursNumeric);
        }

        [Fact]
        public void BuildServiceLabor_SetsTechnicianParty()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.LaborEntries[0].TechnicianNumber = 42;

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.NotNull(result.serviceTechnicianParty);
            Assert.Equal("42", result.serviceTechnicianParty[0].id);
            Assert.Equal(CustomerTypeCodes.PartyIdentifierLocal, result.serviceTechnicianParty[0].type);
        }

        [Fact]
        public void BuildServiceLabor_WithNoMiscCharges_ReturnsNullSubletList()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.MiscCharges = null;

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.Null(result.subletList);
        }

        [Fact]
        public void BuildServiceLabor_WithMiscCharges_ReturnsSubletsWithCorrectPriceCode()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.MiscCharges = new List<MiscCharge>
            {
                new MiscCharge { MiscellaneousChargeID = 100, Description = "Sublet Labor", ExtendedPrice = 50m },
                new MiscCharge { MiscellaneousChargeID = 200, Description = "Sublet Parts", ExtendedPrice = 75m }
            };

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.NotNull(result.subletList);
            Assert.Equal(2, result.subletList.Count);
            Assert.Equal("SubletLabor", result.subletList[0].priceCode);
            Assert.Equal("SubletParts", result.subletList[1].priceCode);
        }

        [Fact]
        public void BuildServiceLabor_WithWarrantyDeductible_CalculatesCorrectCharge()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.LaborEntries[0].ExtendedPrice = 200m;
            task.MiscCharges = new List<MiscCharge>
            {
                new MiscCharge { MiscellaneousChargeID = 300, Description = "Deductible", ExtendedPrice = -50m }
            };

            var result = builder.BuildServiceLabor(task, OperationIds.Warranty, 42, 0, 1, 1);

            Assert.NotNull(result.chargeAmount);
        }

        [Fact]
        public void BuildDeclinedServiceLabor_SetsDeclinedStatus()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask(warranty: true);

            var result = builder.BuildDeclinedServiceLabor(task);

            Assert.Equal(VolvoRepairOrderTaskStatus.DECLINED, result.laborOperationTypeCode);
        }

        [Fact]
        public void BuildDeclinedServiceLabor_WithWarrantyTask_UsesTaskSRTID()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask(warranty: true, srtId: "SRT789");

            var result = builder.BuildDeclinedServiceLabor(task);

            Assert.Equal("SRT789", result.laborOperationId);
        }

        [Fact]
        public void GetSubletPriceCode_WithWarrantyDeductible_ReturnsSubletLabor()
        {
            //_repairOrder.OriginalRepairOrderNumber = "RO999";
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.MiscCharges = new List<MiscCharge>
            {
                new MiscCharge { MiscellaneousChargeID = 100, Description = "Warranty Deductible", ExtendedPrice = -25m }
            };

            var result = builder.BuildServiceLabor(task, OperationIds.Warranty, 42, 0, 1, 1);

            Assert.NotNull(result.subletList);
            Assert.Single(result.subletList);
            Assert.Equal("SubletLabor", result.subletList[0].priceCode);
        }

        [Fact]
        public void GetSubletPriceCode_WithWarrantyDeductible_ReturnsSubletLabor_WithNoLaborRecord()
        {
            //_repairOrder.OriginalRepairOrderNumber = "RO999";
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTaskNoLabor();
            task.MiscCharges = new List<MiscCharge>
            {
                new MiscCharge { MiscellaneousChargeID = 100, Description = "Warranty Deductible", ExtendedPrice = -25m }
            };

            var result = builder.BuildServiceLabor(task, OperationIds.Warranty, null, 0, null, null);

            Assert.NotNull(result.subletList);
            Assert.Single(result.subletList);
            Assert.Equal("SubletLabor", result.subletList[0].priceCode);
        }


        [Fact]
        public void GetSubletDescription_WithSecondaryRO_AndWarrantyDeductible_AddsDedWtyPrefix()
        {
            _repairOrder.OriginalRepairOrderNumber = "RO999";
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.MiscCharges = new List<MiscCharge>
            {
                new MiscCharge { MiscellaneousChargeID = 300, Description = "Deductible", ExtendedPrice = -25m }
            };

            var result = builder.BuildServiceLabor(task, OperationIds.Warranty, 42, 0, 1, 1);

            Assert.NotNull(result.subletList);
            Assert.Single(result.subletList);
            Assert.StartsWith("DED-WTY", result.subletList[0].subletWorkDescription);
        }

        [Fact]
        public void GetSubletDescription_WithSecondaryRO_AndEspDeductible_AddsDedEspPrefix()
        {
            _repairOrder.OriginalRepairOrderNumber = "RO999";
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.MiscCharges = new List<MiscCharge>
            {
                new MiscCharge { MiscellaneousChargeID = 400, Description = "ESP Deductible", ExtendedPrice = -30m }
            };

            var result = builder.BuildServiceLabor(task, OperationIds.ExtendedServicePlan, 42, 0, 1, 1);

            Assert.NotNull(result.subletList);
            Assert.Single(result.subletList);
            Assert.StartsWith("DED-ESP", result.subletList[0].subletWorkDescription);
        }

        [Fact]
        public void BuildServiceLabor_SetsWorkshopCodes()
        {
            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.Equal(_department, result.dealerWorkShopCode);
            Assert.Equal(_department, result.dealerWorkName);
        }

        [Fact]
        public void BuildServiceLabor_WithTaskDepartmentIDandDepartment_UsesTaskDepartmentForWorkshopCode()
        {
            _repairOrder.DepartmentID = 1;
            _volvoSettings.InterfaceOptions.ServiceDepartments = new[] { 1 };
            _volvoSettings.InterfaceOptions.QuickLaneDepartments = new[] { 2 };

            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.DepartmentID = 2;
            task.Department = "Quick Lane";

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.Equal(ServiceDepartments.QUICK_LANE_DEPARTMENT, result.workshopCode);
            Assert.Equal("Quick Lane", result.dealerWorkName);
            Assert.Equal("Quick Lane", result.dealerWorkShopCode);

        }

        [Fact]
        public void BuildServiceLabor_WithNullTaskDepartmentIDandDepartment_FallsBackToRepairOrderDepartment()
        {
            _repairOrder.DepartmentID = 1;
            _volvoSettings.InterfaceOptions.ServiceDepartments = new[] { 1 };
            _volvoSettings.InterfaceOptions.QuickLaneDepartments = new[] { 2 };

            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.DepartmentID = null;
            task.Department = null;

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.Equal(ServiceDepartments.SERVICE_DEPARTMENT, result.workshopCode);
            Assert.Equal("Service", result.dealerWorkName);
            Assert.Equal("Service", result.dealerWorkShopCode);
        }

        [Fact]
        public void BuildServiceLabor_WithEmptyTaskDepartment_FallsBackToRepairOrderDepartment()
        {
            _repairOrder.DepartmentID = 1;
            _volvoSettings.InterfaceOptions.ServiceDepartments = new[] { 1 };
            _volvoSettings.InterfaceOptions.QuickLaneDepartments = new[] { 2 };

            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask();
            task.Department = "";

            var result = builder.BuildServiceLabor(task, OperationIds.Customer, 42, 0, 1, 1);

            Assert.Equal("Service", result.dealerWorkName);
            Assert.Equal("Service", result.dealerWorkShopCode);
        }

        [Fact]
        public void BuildDeclinedServiceLabor_WithTaskDepartmentIDandDepartment_UsesTaskDepartmentForWorkshopCode()
        {
            _repairOrder.DepartmentID = 1;
            _volvoSettings.InterfaceOptions.ServiceDepartments = new[] { 1 };
            _volvoSettings.InterfaceOptions.QuickLaneDepartments = new[] { 2 };

            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask(warranty: true);
            task.DepartmentID = 2;
            task.Department = "Quick Lane";

            var result = builder.BuildDeclinedServiceLabor(task);

            Assert.Equal(ServiceDepartments.QUICK_LANE_DEPARTMENT, result.workshopCode);
            Assert.Equal("Quick Lane", result.dealerWorkName);
            Assert.Equal("Quick Lane", result.dealerWorkShopCode);
        }

        [Fact]
        public void BuildDeclinedServiceLabor_WithNullTaskDepartmentIDandDepartment_FallsBackToRepairOrderDepartment()
        {
            _repairOrder.DepartmentID = 1;
            _volvoSettings.InterfaceOptions.ServiceDepartments = new[] { 1 };
            _volvoSettings.InterfaceOptions.QuickLaneDepartments = new[] { 2 };

            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask(warranty: true);
            task.DepartmentID = null;
            task.Department = null;

            var result = builder.BuildDeclinedServiceLabor(task);

            Assert.Equal(ServiceDepartments.SERVICE_DEPARTMENT, result.workshopCode);
            Assert.Equal("Service", result.dealerWorkName);
            Assert.Equal("Service", result.dealerWorkShopCode);
        }

        [Fact]
        public void BuildDeclinedServiceLabor_WithEmptyTaskDepartment_FallsBackToRepairOrderDepartment()
        {
            _repairOrder.DepartmentID = 1;
            _volvoSettings.InterfaceOptions.ServiceDepartments = new[] { 1 };
            _volvoSettings.InterfaceOptions.QuickLaneDepartments = new[] { 2 };

            var builder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var task = CreateBasicTask(warranty: true);
            task.Department = "";

            var result = builder.BuildDeclinedServiceLabor(task);

            Assert.Equal("Service", result.dealerWorkName);
            Assert.Equal("Service", result.dealerWorkShopCode);
        }

        private RepairOrderTask CreateBasicTask(bool warranty = false, string srtId = "SRT001", string repairType = "RT001", string repairTypeDescription = "Basic Repair")
        {
            return new RepairOrderTask
            {
                Warranty = warranty,
                SRTID = srtId,
                RepairType = repairType,
                RepairTypeDescription = repairTypeDescription,
                LaborEntries = new List<Labor>
                {
                    new Labor { TotalHours = 1.0m, ExtendedPrice = 100m, TechnicianNumber = 1 }
                },
                MiscCharges = new List<MiscCharge>()
            };
        }

        private RepairOrderTask CreateBasicTaskNoLabor(bool warranty = false, string srtId = "SRT001", string repairType = "RT001", string repairTypeDescription = "Basic Repair")
        {
            return new RepairOrderTask
            {
                Warranty = warranty,
                SRTID = srtId,
                RepairType = repairType,
                RepairTypeDescription = repairTypeDescription,
                MiscCharges = new List<MiscCharge>()
            };
        }
    }
}
