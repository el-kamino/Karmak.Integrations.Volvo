using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.React.Mappers.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.Settings
{
    public class RegionSettingsValidator : AbstractValidator<RegionSettings>
    {
        private readonly IEnumerable<string> VALID_LANGUAGE_CODES = new List<string>()
        {
            "en-US",
            "en-CA",
            "es-MX"
        };

        private readonly IEnumerable<string> VALID_CURRENCY_CODES = new List<string>()
        {
            "USD",
            "CAD",
            "MXN"
        };

        public RegionSettingsValidator()
        {
            RuleFor(regionSettings => regionSettings.LanguageCode)
                .Must(languageCode => !string.IsNullOrWhiteSpace(languageCode))
                .Must(languageCode => VALID_LANGUAGE_CODES.Contains(languageCode));

            RuleFor(regionSettings => regionSettings.CountryCode)
                .Must(countryCode => !string.IsNullOrWhiteSpace(countryCode) && !string.IsNullOrWhiteSpace(CountryID.ToAlpha3(countryCode)));

            RuleFor(regionSettings => regionSettings.CurrencyCode)
                .Must(currencyCode => !string.IsNullOrWhiteSpace(currencyCode))
                .Must(currencyCode => VALID_CURRENCY_CODES.Contains(currencyCode));
        }

        public static bool IsValid(RegionSettings regionSettings, out string errors)
        {
            var validationResult = new RegionSettingsValidator().Validate(regionSettings);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            return !validationErrors.Any()
                ? null
                : "Volvo RegionSettings is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }

    public static class RegionSettingsExtension
    {
        public static bool IsValid(this RegionSettings regionSettings, out string errors)
        {
            return RegionSettingsValidator.IsValid(regionSettings, out errors);
        }
    }
}