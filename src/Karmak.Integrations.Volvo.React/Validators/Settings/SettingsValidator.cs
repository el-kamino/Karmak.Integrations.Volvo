using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.Settings
{
    public static class SettingsExtension
    {
        public static bool IsValid(this VolvoSettings volvoSettings, out string errors)
        {
            return SettingsValidator.IsValid(volvoSettings, out errors);
        }
    }

    public class SettingsValidator : AbstractValidator<VolvoSettings>
    {
        private SettingsValidator()
        {
            RuleFor(settings => settings.RegionSettings)
                .NotNull()
                .SetValidator(new RegionSettingsValidator());
            RuleFor(settings => settings.InterfaceOptions)
                .NotNull()
                .SetValidator(new InterfaceOptionsValidator());
        }

        public static bool IsValid(VolvoSettings volvoSettings, out string errors)
        {
            var validationResult = new SettingsValidator().Validate(volvoSettings);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            return !validationErrors.Any()
                ? null
                : "Volvo Settings is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}
