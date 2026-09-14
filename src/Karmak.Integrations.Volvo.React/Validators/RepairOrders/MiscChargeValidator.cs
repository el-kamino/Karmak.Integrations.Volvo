using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class MiscChargeValidatorExtensions
    {
        public static bool IsValid(this MiscCharge MiscCharge, out string errors)
        {
            return MiscChargeValidator.IsValid(MiscCharge, out errors);
        }
    }

    public class MiscChargeValidator : AbstractValidator<MiscCharge>
    {
        public MiscChargeValidator()
        {
            RuleFor(charge => charge.Description)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(charge => charge.ExtendedPrice)
                .NotNull();
            RuleFor(charge => charge.MiscellaneousChargeID)
                .NotNull();
        }

        public static bool IsValid(MiscCharge MiscCharge, out string errors)
        {
            var validationResult = new MiscChargeValidator().Validate(MiscCharge);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "MiscCharge is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}