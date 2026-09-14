using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class AddressValidatorExtensions
    {
        public static bool IsValid(this Address address, out string errors)
        {
            return AddressValidator.IsValid(address, out errors);
        }
    }

    public class AddressValidator : AbstractValidator<Address>
    {
        public AddressValidator()
        {
            RuleFor(address => address.Street1)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(address => address.City)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(address => address.Region)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(address => address.AddressType)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(address => address.EntityType)
                .Must(s => !string.IsNullOrWhiteSpace(s));
        }

        public static bool IsValid(Address address, out string errors)
        {
            var validationResult = new AddressValidator().Validate(address);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "Address is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}