using Karmak.Integrations.Volvo.React.Contracts.Common;
using FluentValidation;
using Karmak.Integrations.Volvo.React.Validators;

namespace Karmak.Integrations.Volvo.React.Validators.PartsSaleOrders
{
    public class ContactValidator : BaseValidator<Contact> {
        public ContactValidator() {
            RuleFor(c => c.LastName)
                .Must(n => !string.IsNullOrWhiteSpace(n))
                .WithMessage("Contact LastName cannot be null or whitespace.");
        }
    }
}