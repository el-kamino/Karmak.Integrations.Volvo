using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.RepairOrders;
using Karmak.Integrations.Volvo.React.Utils;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Karmak.Integrations.Volvo.React.Mappers.RepairOrder._6_0_0
{
    internal class VolvoRepairOrderJobBuilder
    {
        private readonly RepairOrderSnapshot _repairOrder;
        private readonly VolvoSettings _volvoSettings;
        private readonly VolvoRepairOrderState _roState;
        private VolvoRepairOrderPartsBuilder _partsBuilder;
        private VolvoRepairOrderLaborBuilder _laborBuilder;

        public VolvoRepairOrderJobBuilder(RepairOrderSnapshot repairOrder, VolvoSettings volvoSettings, VolvoRepairOrderState roState, VolvoRepairOrderPartsBuilder partsBuilder, VolvoRepairOrderLaborBuilder laborBuilder)
        {
            _repairOrder = repairOrder;
            _volvoSettings = volvoSettings;
            _roState = roState;
            _partsBuilder = partsBuilder;
            _laborBuilder = laborBuilder;
        }

        public List<VolvoJob> BuildVolvoJobs()
        {
            var jobs = new List<VolvoJob>();
            if (_repairOrder.Tasks != null && _repairOrder.Tasks.Any())
            {
                bool isSecondaryRO = !string.IsNullOrWhiteSpace(_repairOrder.OriginalRepairOrderNumber);

                foreach (var task in _repairOrder.Tasks)
                {
                    if (task.LaborEntries == null || !task.LaborEntries.Any())
                    {
                        //no labor, build a job passing nulls for the labor parameters
                        if (BuildJob(task, isSecondaryRO, 0, null, null, null, null, null, _repairOrder.BillingCustomer.CustomerKey) is { } job)
                        {
                            jobs.Add(job);
                        }
                    }
                    else
                    {
                        //If the task has Labor entered for more than one Technician, we need to split this task 
                        //  into multiple subJobs in the Volvo payload, one for each Technician.
                        var laborEntriesByTechnician = task.LaborEntries
                        .GroupBy(labor => labor.TechnicianNumber)
                        .Select(group => new
                        {
                            TechnicianNumber = group.Key,
                            TotalHours = group.Sum(labor => labor.TotalHours),
                            TotalPrice = group.Sum(labor => labor.ExtendedPrice),
                            MinDateTimeIn = group.Min(labor => labor.DateTimeIn),
                            MaxDateTimeOut = group.Max(labor => labor.DateTimeOut)
                        })
                        .ToList();

                        int subJobCounter = 0;
                        foreach (var laborByTech in laborEntriesByTechnician)
                        {
                            if (BuildJob(task, isSecondaryRO, subJobCounter, laborByTech.TechnicianNumber, laborByTech.TotalHours, laborByTech.TotalPrice, laborByTech.MinDateTimeIn, laborByTech.MaxDateTimeOut, _repairOrder.BillingCustomer.CustomerKey) is { } job)
                            {
                                jobs.Add(job);
                                subJobCounter++;
                            }
                        }
                    }
                }
            }
            return jobs;
        }

        private VolvoJob BuildJob(RepairOrderTask task, bool isSecondaryRO, int subJobCounter, int? technicianNumber, decimal? totalHours, decimal? totalPrice, DateTime? minDateIn, DateTime? maxDateOut, string roBillingCustomerKey)
        {
            var operationId = GetOperationIdForCustomerKey(isSecondaryRO, task.AlternateBillingCustomerKey, roBillingCustomerKey);

            if (shouldIncludeThisTask(isSecondaryRO, operationId))
            {
                if (shouldDeleteThisTask(_roState.RepairOrderStatusState.VolvoRoStatus))
                {
                    //Yet another Volvo hack - if the RO is in a status Arrived or Canceled we will send no tasks
                    //BUT we will also not send an RO to Volvo that has no tasks.  
                    //So add an empty Job that will get deleted later, no reason to build it all up since it will get deleted.
                    return new VolvoJob
                    {
                        jobNumberString = "INVALID"
                    };
                }

                string taskStatus = _roState.RepairOrderStatusState.TaskStatusStates.FirstOrDefault(t => t.TaskNumber == task.TaskNumber)?.VolvoRoTaskStatus;

                if (taskStatus == VolvoRepairOrderTaskStatus.DECLINED)
                {
                    return BuildDeclinedJob(task, isSecondaryRO, roBillingCustomerKey, operationId);
                }

                string paymentMethod = String.IsNullOrWhiteSpace(_repairOrder.PaymentMethod) ? "Unknown" : _repairOrder.PaymentMethod.Trim().MaxLength(50);

                return new VolvoJob
                {
                    jobNumberString = subJobCounter.ToString().PadLeft(2, '0'), //"00" for all unless it is split into subJobs, then "00", "01", "02", etc.
                    operationId = operationId,
                    lineNumber = task.TaskNumber.ToString(),
                    jobStatusCode = taskStatus,
                    technicianStartsJobSignInDateTime = DateTimeUtility.ApplyCustomTimeZone(minDateIn, _repairOrder.TimeZone),
                    technicianFinishesJobSignOutDateTime = DateTimeUtility.ApplyCustomTimeZone(maxDateOut, _repairOrder.TimeZone),
                    codesAndCommentsExpanded = BuildCodesAndComments(task),
                    dealerPaymentName = paymentMethod,
                    dealerPaymentCode = paymentMethod,
                    serviceParts = subJobCounter == 0 ? _partsBuilder.BuildServiceParts(task) : null, //only parts on first subJob
                    serviceLabor = _laborBuilder.BuildServiceLabor(task, operationId, technicianNumber, subJobCounter, totalHours, totalPrice)
                };
            }
            return null;
        }

        private VolvoJob BuildDeclinedJob(RepairOrderTask task, bool isSecondaryRO, string roBillingCustomerKey, string operationId)
        {
            return new VolvoJob
            {
                jobNumberString = "00",
                operationId = operationId,
                lineNumber = task.TaskNumber.ToString(),
                jobStatusCode = VolvoRepairOrderTaskStatus.DECLINED,
                serviceLabor = _laborBuilder.BuildDeclinedServiceLabor(task)
            };
        }

        public bool shouldDeleteThisTask(string roStatus)
        {
            //we will delete all tasks for Arrived or Canceled
            if (roStatus == VolvoRepairOrderStatus.ARRIVED || roStatus == VolvoRepairOrderStatus.CANCELED)
            {
                return true;
            }
            return false;
        }

        public bool shouldIncludeThisTask(bool isSecondaryRO, string operationID)
        {
            //operationID will be Unknown if this task is NOT Volvo Warranty, ESP, AWA, Internal, or Customer Pay.
            //We assume these are "some other" Warranty
            if (operationID == OperationIds.Unknown)
            {
                return false;
            }

            //We do not include tasks that will be split off to a new RO, as those tasks will be included in the new RO's payload.
            //So send if this is a split RO, or the original RO and it's Customer Pay.
            return isSecondaryRO || operationID == OperationIds.Customer;
        }

        private string GetOperationIdForCustomerKey(bool isSecondaryRO, string alternateBillingCutomerKey, string roBillingCustomer)
        {
            if (alternateBillingCutomerKey == roBillingCustomer)
            {
                //if the alt billing customer key matches the billing customer, this task will not split
                //and should be included in the original RO with an operation ID of Customer
                return OperationIds.Customer;
            }

            return (!isSecondaryRO, string.IsNullOrWhiteSpace(alternateBillingCutomerKey)) switch
            {
                (true, true) => OperationIds.Customer,
                _ when IsCustomerKeyInList(isSecondaryRO, alternateBillingCutomerKey, roBillingCustomer, _volvoSettings.InterfaceOptions.WarrantyCustomers) => OperationIds.Warranty,
                _ when IsCustomerKeyInList(isSecondaryRO, alternateBillingCutomerKey, roBillingCustomer, _volvoSettings.InterfaceOptions.EspCustomers) => OperationIds.ExtendedServicePlan,
                _ when IsCustomerKeyInList(isSecondaryRO, alternateBillingCutomerKey, roBillingCustomer, _volvoSettings.InterfaceOptions.AwaCustomers) => OperationIds.AfterWarrantyAssistance,
                _ when IsCustomerKeyInList(isSecondaryRO, alternateBillingCutomerKey, roBillingCustomer, _volvoSettings.InterfaceOptions.InternalPolicyCustomers) => OperationIds.InternalPolicyCustomer,
                _ => OperationIds.Unknown
            };
        }

        private bool IsCustomerKeyInList(bool isSecondaryRO, string alternateBillingCutomerKey, string roBillingCustomer, string[] list)
        {
            return list.NotNullAndContains(alternateBillingCutomerKey)
                || (isSecondaryRO && list.NotNullAndContains(roBillingCustomer));
        }

        private static VolvoCodesAndCommentsExpanded BuildCodesAndComments(RepairOrderTask task)
        {
            var codesAndComments = new VolvoCodesAndCommentsExpanded
            {
                causeDescription = task.CauseDescription.WafSanitize().MaxLength(8000),
                complaintDescription = task.ComplaintDescription.WafSanitize().MaxLength(8000),
                correctionDescription = task.CorrectionDescription.WafSanitize().MaxLength(8000),
            };
            return codesAndComments;
        }


    }
}
