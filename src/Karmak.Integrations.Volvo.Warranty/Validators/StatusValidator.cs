using FluentValidation;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Validators
{
    public class StatusValidator : AbstractValidator<UpdateSnapshotMessage>
    {
        public StatusValidator()
        {
            RuleFor(status => status.ClaimId)
                .Custom((claimId, context) =>
                {
                    if (claimId == null)
                    {
                        context.AddFailure("Failed to correlate status update with claim.");
                    }
                });
            RuleFor(status => status.UpdateSnapshot).SetValidator(new StatusUpdateValidator());
        }

        private class StatusUpdateValidator : AbstractValidator<UpdateSnapshot>
        {
            public StatusUpdateValidator()
            {
                RuleFor(status => status.RepairOrderNumber)
                .NotNull()
                .NotEmpty();
                RuleFor(status => status.DealerCode)
                    .NotNull()
                    .NotEmpty();
                RuleFor(status => status.Status)
                    .NotNull();
                When(status => (status.Status != null), () =>
                {
                    RuleFor(status => status.Status.Code)
                        .NotNull()
                        .NotEmpty();
                    RuleFor(status => status.Status.Description)
                        .NotNull()
                        .NotEmpty();
                });
            }
        }
    }
}
