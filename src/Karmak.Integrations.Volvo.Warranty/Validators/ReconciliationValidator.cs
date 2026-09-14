using FluentValidation;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Validators
{
    public class ReconciliationValidator : AbstractValidator<UpdateSnapshotMessage>
    {
        public ReconciliationValidator()
        {
            RuleFor(reconcilaition => reconcilaition.ClaimId)
                .Custom((claimId, context) =>
                {
                    if (claimId == null)
                    {
                        context.AddFailure("Failed to correlate reconciliation update with claim.");
                    }
                });
            RuleFor(reconciliation => reconciliation.UpdateSnapshot)
                .NotNull()
                .SetValidator(new ReconciliationUpdateValidator());
        }

        private class ReconciliationUpdateValidator : AbstractValidator<UpdateSnapshot>
        {
            public ReconciliationUpdateValidator()
            {
                RuleFor(reconciliation => reconciliation.RepairOrderNumber)
                    .NotNull()
                    .NotEmpty();
                RuleFor(reconciliation => reconciliation.DealerCode)
                    .NotNull()
                    .NotEmpty();
                RuleFor(reconciliation => reconciliation.ApprovedAmount)
                    .NotNull();
            }
        }
    }
}
