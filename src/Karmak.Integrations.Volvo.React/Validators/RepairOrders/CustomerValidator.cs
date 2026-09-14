using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class CustomerValidatorExtensions
    {
        public static bool IsValid(this Customer customer, out string errors)
        {
            return CustomerValidator.IsValid(customer, out errors);
        }
    }

    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(customer => customer.CustomerKey)
                .Must(s => !string.IsNullOrWhiteSpace(s));
            RuleFor(customer => customer.Contacts)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleForEach(customer => customer.Contacts)
                        .SetValidator(new ContactValidator());
                });
        }

        public static bool IsValid(Customer customer, out string errors)
        {
            var validationResult = new CustomerValidator().Validate(customer);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "Customer is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}