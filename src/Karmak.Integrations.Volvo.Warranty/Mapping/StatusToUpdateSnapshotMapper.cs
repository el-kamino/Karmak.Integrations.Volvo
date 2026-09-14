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
    public class StatusToUpdateSnapshotMapper : IStatusToUpdateSnapshotMapper
    {
        public UpdateSnapshot Map(RepairOrderReconciliationType source, DateTime processDate, string paCode)
        {
            var jobReconciliation = source.JobReconciliation?.FirstOrDefault();
            var dispositionReason = jobReconciliation?.DispositionReason?.FirstOrDefault();

            return new UpdateSnapshot
            {
                Type = Contracts.UpdateType.Status,
                RepairOrderNumber = source.DocumentID?.Value,
                ProcessDate = processDate,
                DealerCode = paCode,
                ApprovedAmount = Conversions.AmountTypeToMoney.Convert(jobReconciliation?.ApprovedAmount),
                Status = MapStatus(dispositionReason),
                Exceptions = MapExceptions(dispositionReason?.ExceptionCodes),
                PartsAmount = null,
                LaborAmount = null,
                OtherAmount = null,
                Taxes = new List<Tax>(),
                PartExpenses = new List<ReconciliationPartExpense>(),
                LaborExpenses = new List<ReconciliationLaborExpense>(),
                MiscellaneousExpenses = new List<ReconciliationMiscellaneousExpense>(),
                Deductibles = new List<Deductible>(),
                Id = ResolveStatusId(source, processDate.ToString(), paCode),
                OemId = ResolveStatusId(source, processDate.ToString(), paCode),
                CreatedDateTime = DateTime.UtcNow
            };
        }

        private static UpdateStatus MapStatus(DispositionReasonExtended dispositionReason)
        {
            if (dispositionReason == null) return null;

            return new UpdateStatus
            {
                Code = dispositionReason.DispositionStatusCode,
                Description = dispositionReason.DispositionStatusString
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

        private static string ResolveStatusId(RepairOrderReconciliationType source, string processDate, string paCode)
        {
            var repairOrderNumber = source?.DocumentID?.Value;
            var statusCode = source?.JobReconciliation?.FirstOrDefault()?.DispositionReason?.FirstOrDefault()?.DispositionStatusCode;

            return (processDate == null) || (paCode == null) || (repairOrderNumber == null) || (statusCode == null)
                ? null
                : processDate + "-" + paCode + "-" + repairOrderNumber + "-" + statusCode;
        }
    }
}
