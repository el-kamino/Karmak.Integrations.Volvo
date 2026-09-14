using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;
using ReconciliationLaborExpense = Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation.LaborExpense;
using ReconciliationMiscellaneousExpense = Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation.MiscellaneousExpense;
using ReconciliationPartExpense = Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation.PartExpense;

namespace Karmak.Integrations.Volvo.Warranty.Mapping
{
    public class ReconciliationToUpdateSnapshotMapper : IReconciliationToUpdateSnapshotMapper
    {
        public UpdateSnapshot Map(RepairOrderReconciliationType source, DateTime processDate, string paCode, Claim claim, DateTime createdDateTime)
        {
            var jobReconciliation = source.JobReconciliation?.FirstOrDefault();
            var warrantyClaimReconciliation = source.WarrantyClaimReconciliation?.FirstOrDefault();

            return new UpdateSnapshot
            {
                Type = Contracts.UpdateType.Reconciliation,
                RepairOrderNumber = source.DocumentID?.Value,
                ProcessDate = processDate,
                DealerCode = paCode,
                CreatedDateTime = DateTime.UtcNow,
                ApprovedAmount = Conversions.AmountTypeToMoney.Convert(jobReconciliation?.ApprovedAmount),
                LaborAmount = Conversions.AmountTypeToMoney.Convert(jobReconciliation?.LaborAmount),
                PartsAmount = Conversions.AmountTypeToMoney.Convert(jobReconciliation?.PartsAmount),
                OtherAmount = Conversions.AmountTypeToMoney.Convert(jobReconciliation?.OtherAmount),
                Exceptions = MapExceptions(jobReconciliation?.DispositionReason?.FirstOrDefault()?.ExceptionCodes),
                Taxes = MapTaxes(jobReconciliation?.Tax),
                MiscellaneousExpenses = MapMiscellaneousExpenses(jobReconciliation?.ServiceComponents, claim),
                LaborExpenses = MapLaborExpenses(jobReconciliation?.ServiceLabor, claim),
                PartExpenses = MapPartExpenses(jobReconciliation?.ServiceParts, claim),
                Deductibles = MapDeductibles(warrantyClaimReconciliation?.WarrantyClaimDeductible),
                Status = MapStatusFromCode(jobReconciliation?.ClaimStatusCode),
                Id = ResolveReconciliationId(source, processDate.ToString(), paCode, createdDateTime.ToString()),
                OemId = ResolveOemId(source, processDate.ToString(), paCode)
            };
        }

        private static IEnumerable<UpdateException> MapExceptions(ExceptionCodesType[] exceptionCodes)
        {
            if (exceptionCodes == null) return Enumerable.Empty<UpdateException>();

            return exceptionCodes.Select(ec => new UpdateException
            {
                Code = ec.Code?.Value,
                Description = ec.ExceptionText?.Value
            });
        }

        private static IEnumerable<Tax> MapTaxes(TaxType[] taxes)
        {
            if (taxes == null) return Enumerable.Empty<Tax>();

            return taxes.Select(t => new Tax
            {
                Amount = Conversions.AmountTypeToMoney.Convert(t.TaxAmount),
                Description = t.TaxDescription?.Select(td => td.Value),
                RatePercent = t.TaxRatePercent
            });
        }

        private static IEnumerable<ReconciliationMiscellaneousExpense> MapMiscellaneousExpenses(ServiceComponentsType1[] components, Claim claim)
        {
            if (components == null) return Enumerable.Empty<ReconciliationMiscellaneousExpense>();

            return components.Select(component => ConvertMiscellaneousExpense(component, claim));
        }

        private static ReconciliationMiscellaneousExpense ConvertMiscellaneousExpense(ServiceComponentsType1 source, Claim claim)
        {
            var miscellaneousExpenses = claim?.RepairOrder?.Jobs?.First()?.MiscellaneousExpenses;
            var matchedExpense = miscellaneousExpenses?.Where(expense => expense.Identifier == source.ComponentTypeCode?.Value);

            return matchedExpense?.Count() == 1
                ? new ReconciliationMiscellaneousExpense
                {
                    Amount = Conversions.AmountTypeToMoney.Convert(source.ServiceComponentAmount),
                    TypeCodeValue = source.ComponentTypeCode?.Value,
                    RequestedAmount = matchedExpense.First().Total,
                    Description = matchedExpense.First().Description
                }
                : new ReconciliationMiscellaneousExpense
                {
                    Amount = Conversions.AmountTypeToMoney.Convert(source.ServiceComponentAmount),
                    TypeCodeValue = source.ComponentTypeCode?.Value,
                    RequestedAmount = null,
                    Description = string.Empty
                };
        }

        private static IEnumerable<ReconciliationLaborExpense> MapLaborExpenses(ServiceLaborType1[] laborItems, Claim claim)
        {
            if (laborItems == null) return Enumerable.Empty<ReconciliationLaborExpense>();

            return laborItems.Select(labor => ConvertLaborExpense(labor, claim));
        }

        private static ReconciliationLaborExpense ConvertLaborExpense(ServiceLaborType1 source, Claim claim)
        {
            var laborExpenses = claim?.RepairOrder?.Jobs?.First()?.LaborExpenses;
            var matchedExpense = laborExpenses?.Where(expense => expense.Identifier == source.LaborOperationID?.Value);

            return matchedExpense?.Count() == 1
                ? new ReconciliationLaborExpense
                {
                    OperationId = source.LaborOperationID?.Value,
                    Amount = Conversions.AmountTypeToMoney.Convert(source.LaborAmount),
                    RequestedAmount = matchedExpense.First().Total,
                    Description = matchedExpense.First().Description,
                    Hours = new Quantity
                    {
                        Value = source.LaborActualHoursNumeric,
                        Type = UnitOfMeasureType.Hours
                    },
                    RequestedHours = new Quantity
                    {
                        Value = matchedExpense.First().Quantity?.Value ?? 0,
                        Type = UnitOfMeasureType.Hours
                    }
                }
                : new ReconciliationLaborExpense
                {
                    OperationId = source.LaborOperationID?.Value,
                    Amount = Conversions.AmountTypeToMoney.Convert(source.LaborAmount),
                    RequestedAmount = null,
                    Description = string.Empty,
                    Hours = new Quantity
                    {
                        Value = source.LaborActualHoursNumeric,
                        Type = UnitOfMeasureType.Hours
                    },
                    RequestedHours = null
                };
        }

        private static IEnumerable<ReconciliationPartExpense> MapPartExpenses(ServicePartsType1[] parts, Claim claim)
        {
            if (parts == null) return Enumerable.Empty<ReconciliationPartExpense>();

            return parts.Select(part => ConvertPartExpense(part, claim));
        }

        private static ReconciliationPartExpense ConvertPartExpense(ServicePartsType1 source, Claim claim)
        {
            var partExpenses = claim?.RepairOrder?.Jobs?.First()?.PartExpenses;
            var matchedExpense = partExpenses?.Where(expense => expense.Identifier == source.ItemIdentification?.Value);

            var quantityType = Enum.TryParse(source.ItemQuantity?.unitCode, out UnitOfMeasureType parsedType)
                ? parsedType
                : UnitOfMeasureType.Count;
            var quantity = new Quantity
            {
                Type = quantityType,
                Value = source.ItemQuantity?.Value ?? 0
            };

            return matchedExpense?.Count() == 1
                ? new ReconciliationPartExpense
                {
                    Amount = Conversions.AmountTypeToMoney.Convert(source.PartAmount),
                    ItemId = source.ItemIdentification?.Value,
                    RequestedAmount = matchedExpense.First().Total,
                    Description = matchedExpense.First().Description,
                    Quantity = quantity,
                    RequestedQuantity = matchedExpense.First().Quantity
                }
                : new ReconciliationPartExpense
                {
                    Amount = Conversions.AmountTypeToMoney.Convert(source.PartAmount),
                    ItemId = source.ItemIdentification?.Value,
                    RequestedAmount = null,
                    Description = string.Empty,
                    Quantity = quantity,
                    RequestedQuantity = null
                };
        }

        private static IEnumerable<Deductible> MapDeductibles(WarrantyClaimDeductibleType[] deductibles)
        {
            if (deductibles == null) return Enumerable.Empty<Deductible>();

            return deductibles.Select(d => new Deductible
            {
                Amount = Conversions.AmountTypeToMoney.Convert(d.DeductibleAmount),
                Type = d.DeductibleTypeString
            });
        }

        private static UpdateStatus MapStatusFromCode(string statusCode)
        {
            if (statusCode == null) return null;

            var trimmedUpper = statusCode.Trim().ToUpper();
            return new UpdateStatus
            {
                Code = statusCode,
                Description = MappingConstants.StatusDescriptions.ContainsKey(trimmedUpper)
                    ? MappingConstants.StatusDescriptions[trimmedUpper]
                    : MappingConstants.DefaultStatusDescription
            };
        }

        private static string ResolveReconciliationId(RepairOrderReconciliationType source, string processDate, string paCode, string createdDate)
        {
            var repairOrderNumber = source?.DocumentID?.Value;
            var jobNumber = source?.JobReconciliation?.FirstOrDefault()?.JobNumberString;

            return (createdDate == null) || (processDate == null) || (paCode == null) || (repairOrderNumber == null) || (jobNumber == null)
                ? null
                : string.Join("-", createdDate, processDate, paCode, repairOrderNumber, jobNumber);
        }

        private static string ResolveOemId(RepairOrderReconciliationType source, string processDate, string paCode)
        {
            var repairOrderNumber = source?.DocumentID?.Value;
            var jobNumber = source?.JobReconciliation?.FirstOrDefault()?.JobNumberString;

            return (processDate == null) || (paCode == null) || (repairOrderNumber == null) || (jobNumber == null)
                ? null
                : string.Join("-", processDate, paCode, repairOrderNumber, jobNumber);
        }
    }
}
