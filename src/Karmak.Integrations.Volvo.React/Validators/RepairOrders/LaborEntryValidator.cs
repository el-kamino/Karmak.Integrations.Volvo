using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class LaborValidatorExtensions
    {
        public static bool IsValid(this Labor labor, out string errors)
        {
            return LaborValidator.IsValid(labor, out errors);
        }
    }

    public class LaborValidator : AbstractValidator<Labor>
    {
        public LaborValidator()
        {
            RuleFor(labor => labor.TechnicianNumber)
                .NotNull();
        }

        public static bool IsValid(Labor Labor, out string errors)
        {
            var validationResult = new LaborValidator().Validate(Labor);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "Labor is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}