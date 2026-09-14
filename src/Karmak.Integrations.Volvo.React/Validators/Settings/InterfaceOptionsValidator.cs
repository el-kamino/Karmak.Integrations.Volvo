using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.Settings
{
    public class InterfaceOptionsValidator : AbstractValidator<InterfaceOptions>
    {
        private const int PA_CODE_VALID_LENGTH_FIVE = 5;
        private const int PA_CODE_VALID_LENGTH_SEVEN = 7;
        public InterfaceOptionsValidator()
        {
            RuleFor(interfaceOptions => interfaceOptions.FranchiseCode)
                .Must(franchiseCode => !string.IsNullOrWhiteSpace(franchiseCode));

            RuleFor(interfaceOptions => interfaceOptions.PaCode)
                .Must(paCode => paCode?.Length == PA_CODE_VALID_LENGTH_FIVE || paCode?.Length == PA_CODE_VALID_LENGTH_SEVEN);
        }

        public static bool IsValid(InterfaceOptions interfaceOptions, out string errors)
        {
            var validationResult = new InterfaceOptionsValidator().Validate(interfaceOptions);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            return !validationErrors.Any()
                ? null
                : "Volvo InterfaceOptions is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }

    public static class InterfaceOptionsExtension
    {
        public static bool IsValid(this InterfaceOptions interfaceOptions, out string errors)
        {
            return InterfaceOptionsValidator.IsValid(interfaceOptions, out errors);
        }
    }
}