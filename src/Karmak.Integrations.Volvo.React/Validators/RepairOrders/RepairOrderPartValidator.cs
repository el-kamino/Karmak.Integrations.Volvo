using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class PartValidatorExtensions
    {
        public static bool IsValid(this Part repairOrderPart, out string errors)
        {
            return RepairOrderPartValidator.IsValid(repairOrderPart, out errors);
        }
    }

    public class RepairOrderPartValidator : AbstractValidator<Part>
    {
        public RepairOrderPartValidator()
        {
            RuleFor(part => part.PartNumber)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(part => part.ExtendedPrice)
                .NotNull();
            RuleFor(part => part.Quantity)
                .NotNull();
            RuleFor(part => part.CoreUnitPrice)
                .NotNull();
            RuleFor(part => part.CoreExtendedPrice)
                .NotNull();
            RuleFor(part => part.CoreQuantity)
                .NotNull();
            RuleFor(part => part.UnitCost)
                .NotNull();
            RuleFor(part => part.UnitPrice)
                .NotNull();
            RuleFor(part => part.CoreExtendedCost)
                .NotNull();
            RuleFor(part => part.CoreUnitCost)
                .NotNull();
            RuleForEach(part => part.AssemblyParts)
                .SetValidator(new AssemblyPartValidator());
        }

        public static bool IsValid(Part repairOrderPart, out string errors)
        {
            var validationResult = new RepairOrderPartValidator().Validate(repairOrderPart);
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