using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class AssemblyPartValidatorExtensions
    {
        public static bool IsValid(this AssemblyPart assemblyPart, out string errors)
        {
            return AssemblyPartValidator.IsValid(assemblyPart, out errors);
        }
    }

    public class AssemblyPartValidator : AbstractValidator<AssemblyPart>
    {
        public AssemblyPartValidator()
        {
            RuleFor(assemblyPart => assemblyPart.Description)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(assemblyPart => assemblyPart.PartNumber)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(assemblyPart => assemblyPart.UnitListPrice)
                .NotNull();
            RuleFor(assemblyPart => assemblyPart.Quantity)
                .NotNull();
        }

        public static bool IsValid(AssemblyPart assemblyPart, out string errors)
        {
            var validationResult = new AssemblyPartValidator().Validate(assemblyPart);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "Part is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}