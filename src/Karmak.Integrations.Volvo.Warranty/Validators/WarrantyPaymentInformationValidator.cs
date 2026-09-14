using FluentValidation;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Validators
{
    public class WarrantyPaymentInformationValidator : AbstractValidator<WarrantyPaymentInformation>
    {
        public WarrantyPaymentInformationValidator()
        {
            RuleFor(payment => payment.RepairOrderNumber)
                .NotNull()
                .NotEmpty();
            RuleFor(payment => payment.ClaimNumber)
                .NotNull()
                .NotEmpty();
            RuleFor(payment => payment.DealerCode)
                .NotNull()
                .NotEmpty();
        }
    }
}
