using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class RepairOrderTaskExtensions
    {
        public static bool IsValid(this RepairOrderTask repairOrderTask, out string errors)
        {
            return RepairOrderTaskValidator.IsValid(repairOrderTask, out errors);
        }
    }

    public class RepairOrderTaskValidator : AbstractValidator<RepairOrderTask>
    {
        public RepairOrderTaskValidator()
        {
            RuleFor(repairOrderTask => repairOrderTask.TaskNumber)
                .NotNull();

            RuleFor(repairOrderTask => repairOrderTask.RepairTaskStatus)
                .Must(s => !string.IsNullOrWhiteSpace(s));

            RuleForEach(repairOrderTask => repairOrderTask.Parts)
                .SetValidator(new RepairOrderPartValidator())
                .When(repairOrderTask => repairOrderTask.Parts != null);

            RuleForEach(repairOrderTask => repairOrderTask.LaborEntries)
                .SetValidator(new LaborValidator())
                .When(repairOrderTask => repairOrderTask.LaborEntries != null);

            RuleForEach(repairOrderTask => repairOrderTask.MiscCharges)
                .SetValidator(new MiscChargeValidator())
                .When(repairOrderTask => repairOrderTask.MiscCharges != null);

            RuleFor(repairOrderTask => repairOrderTask.RepairTypeDescription)
                .Must(s => !string.IsNullOrWhiteSpace(s));
        }

        public static bool IsValid(RepairOrderTask repairOrderTask, out string errors)
        {
            var validationResult = new RepairOrderTaskValidator().Validate(repairOrderTask);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "Task is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}