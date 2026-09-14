using System;
using FluentValidation;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;

namespace Karmak.Integrations.Volvo.Warranty.Validators
{
    public class ReconciliationFetchRequestValidator : AbstractValidator<ReconciliationFetchRequest>
    {
        public ReconciliationFetchRequestValidator()
        {
            RuleFor(reconciliationFetchRequest => reconciliationFetchRequest.PACode).NotNull().NotEmpty();
            RuleFor(reconciliationFetchRequest => reconciliationFetchRequest)
                .Must(dateRange => dateRange.StartDateTime <= dateRange.EndDateTime);
            RuleFor(reconciliationFetchRequest => reconciliationFetchRequest.StartDateTime)
                .Must(date => date != default(DateTime));
            RuleFor(reconciliationFetchRequest => reconciliationFetchRequest.EndDateTime)
                .Must(date => date != default(DateTime));
        }
    }
}
