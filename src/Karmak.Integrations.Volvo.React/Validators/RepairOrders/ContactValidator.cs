using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using FluentValidation;
using FluentValidation.Results;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class ContactValidatorExtensions
    {
        public static bool IsValid(this Contact contact, out string errors)
        {
            return ContactValidator.IsValid(contact, out errors);
        }
    }

    public class ContactValidator : AbstractValidator<Contact>
    {
        public ContactValidator()
        {
            RuleFor(contact => contact.LastName)
                .Must(s => !string.IsNullOrWhiteSpace(s));
        }

        public static bool IsValid(Contact contact, out string errors)
        {
            var validationResult = new ContactValidator().Validate(contact);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "Contact is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}