using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.Gen.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.RepairOrders;
using Karmak.Integrations.Volvo.React.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Mappers.RepairOrder._6_0_0
{
    internal class VolvoRepairOrderLaborBuilder
    {
        private readonly RepairOrderSnapshot _repairOrder;
        private VolvoSettings _volvoSettings;

        private enum VolvoAdjustedLaborType
        {
            Regular,
            Kit,
            Exchange,
            Core,
            CoreReturn,
            AssemblyPart,
            AssemblyCharge
        }

        public VolvoRepairOrderLaborBuilder(RepairOrderSnapshot repairOrder, VolvoSettings volvoSettings)
        {
            _repairOrder = repairOrder;
            _volvoSettings = volvoSettings;
        }

        public VolvoServiceLabor BuildServiceLabor(RepairOrderTask task, string operationId, int? technicianNumber, int subJobCounter, decimal? totalHours, decimal? totalPrice)
        {
            //If this is a split Task, only this first subJob (subJobCounter == 0) sublets and labor charge deductibles.
            return new VolvoServiceLabor
            {
                laborOperationId = task.Warranty == true ? task.SRTID : task.RepairType,
                laborOperationDescription = task.RepairTypeDescription.WafSanitize().MaxLength(256),
                chargeAmount = subJobCounter == 0 ? GetLaborCharge(totalPrice, task, operationId).WithDecimalImplied() : totalPrice.GetValueOrDefault().WithDecimalImplied(), //only apply deductible on first subJob
                laborActualHoursNumeric = totalHours.GetValueOrDefault().WithDecimalImplied(1, DecimalExtensions.Round),
                serviceTechnicianParty = new List<VolvoPartyIdentifier>
                {
                    new() {
                        id = technicianNumber.ToString().WafSanitize().MaxLength(100),
                        type = CustomerTypeCodes.PartyIdentifierLocal
                    }
                },
                subletList = subJobCounter == 0 ? GetSubletsIfAny(task) : null,  //only sublet on first subJob
                workshopCode = ServiceDepartments.GetDepartmentCode(task.DepartmentID ?? _repairOrder.DepartmentID, _volvoSettings.InterfaceOptions),
                dealerWorkShopCode = GetDepartmentDescription(task.Department, _repairOrder.Department).WafSanitize().MaxLength(50),
                dealerWorkName = GetDepartmentDescription(task.Department, _repairOrder.Department).WafSanitize().MaxLength(50)
            };
        }

        public VolvoServiceLabor BuildDeclinedServiceLabor(RepairOrderTask task)
        {
            return new VolvoServiceLabor
            {
                laborOperationId = task.Warranty == true ? task.SRTID : task.RepairType,
                laborOperationTypeCode = VolvoRepairOrderTaskStatus.DECLINED,
                workshopCode = ServiceDepartments.GetDepartmentCode(task.DepartmentID ?? _repairOrder.DepartmentID, _volvoSettings.InterfaceOptions),
                dealerWorkShopCode = GetDepartmentDescription(task.Department,_repairOrder.Department).WafSanitize().MaxLength(50),
                dealerWorkName = GetDepartmentDescription(task.Department, _repairOrder.Department).WafSanitize().MaxLength(50)
            };
        }

        private string GetDepartmentDescription(string taskDepartment, string roDepartment)
        {
            if (!string.IsNullOrWhiteSpace(taskDepartment))
                return taskDepartment;
            return roDepartment;
        }

        private List<VolvoSublet> GetSubletsIfAny(RepairOrderTask task)
        {
            if (task.MiscCharges == null || !task.MiscCharges.Any())
                return null;

            return task.MiscCharges.Select(miscCharge => new VolvoSublet
            {
                priceCode = GetSubletPriceCode(miscCharge.MiscellaneousChargeID.GetValueOrDefault()),
                chargeAmount = GetSubletCharge(miscCharge).GetValueOrDefault().WithDecimalImplied(),
                subletWorkDescription = GetSubletDescription(miscCharge).WafSanitize().MaxLength(15)
            }).ToList();
        }

        private string GetSubletPriceCode(int id)
        {
            if (_volvoSettings.InterfaceOptions.SubletLaborCharges.NotNullAndContains(id))
                return "SubletLabor";

            if (_volvoSettings.InterfaceOptions.WarrantyCustomers.NotNullAndContains(_repairOrder.BillingCustomer.CustomerKey)
                && _volvoSettings.InterfaceOptions.WarrantyDeductibleCharges.NotNullAndContains(id))
                return "SubletLabor";

            if (_volvoSettings.InterfaceOptions.EspCustomers.NotNullAndContains(_repairOrder.BillingCustomer.CustomerKey)
                && _volvoSettings.InterfaceOptions.EspDeductibleCharges.NotNullAndContains(id))
                return "SubletLabor";

            if (_volvoSettings.InterfaceOptions.SubletPartsCharges.NotNullAndContains(id))
                return "SubletParts";

            return "Miscellaneous";
        }

        private decimal? GetSubletCharge(MiscCharge miscCharge)
        {
            int id = miscCharge.MiscellaneousChargeID.GetValueOrDefault();
            if (_volvoSettings.InterfaceOptions.WarrantyDeductibleCharges.NotNullAndContains(id) ||
                _volvoSettings.InterfaceOptions.EspDeductibleCharges.NotNullAndContains(id))
                return Math.Abs(miscCharge.ExtendedPrice.GetValueOrDefault());

            return miscCharge.ExtendedPrice;
        }

        private string GetSubletDescription(MiscCharge miscCharge)
        {
            var isSecondaryRO = !string.IsNullOrWhiteSpace(_repairOrder.OriginalRepairOrderNumber);

            if (isSecondaryRO && miscCharge.MiscellaneousChargeID.HasValue)
            {
                if (_volvoSettings.InterfaceOptions.WarrantyDeductibleCharges.NotNullAndContains(miscCharge.MiscellaneousChargeID.Value))
                {
                    return $"DED-WTY {miscCharge.Description}";
                }

                if (_volvoSettings.InterfaceOptions.EspDeductibleCharges.NotNullAndContains(miscCharge.MiscellaneousChargeID.Value))
                {
                    return $"DED-ESP {miscCharge.Description}";
                }
            }

            return miscCharge.Description;
        }

        private decimal GetLaborCharge(decimal? totalPrice, RepairOrderTask task, string operationId)
        {
            return totalPrice.GetValueOrDefault() - GetDeductibleIfAny(task, operationId);
        }

        private decimal GetDeductibleIfAny(RepairOrderTask task, string operationId)
        {
            int[] deductibleMiscChargeIds;
            switch (operationId)
            {
                case OperationIds.Warranty:
                    deductibleMiscChargeIds = _volvoSettings.InterfaceOptions.WarrantyDeductibleCharges;
                    break;
                case OperationIds.ExtendedServicePlan:
                    deductibleMiscChargeIds = _volvoSettings.InterfaceOptions.EspDeductibleCharges;
                    break;
                default:
                    return 0;
            }
            IEnumerable<MiscCharge> deductibleCharges = (from id in deductibleMiscChargeIds
                                                         join miscCharge in task.MiscCharges
                                                         on id equals miscCharge.MiscellaneousChargeID
                                                         select miscCharge).ToList();
            if (deductibleCharges == null)
                return 0;
            return deductibleCharges.Sum(charge => Math.Abs(charge.ExtendedPrice.GetValueOrDefault()));
        }
    }
}
