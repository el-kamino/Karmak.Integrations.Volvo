using FluentValidation;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.PartsSalesOrders.Extensions;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.PartsSaleOrders
{
    public class CustomerValidator : BaseValidator<Customer> {
        public CustomerValidator() {
            When(customer => customer.IsIndividual(), () => {
                RuleFor(customer => customer.Contacts)
                    .NotEmpty()
                    .DependentRules(() => {
                        RuleFor(customer => customer.Contacts.FirstOrDefault())
                            .SetValidator(new ContactValidator());
                    });
            }).Otherwise(() =>
            {
                RuleFor(customer => customer.CompanyName)
                    .Must(name => !string.IsNullOrWhiteSpace(name))
                    .WithMessage("Customer is an Organization and is missing a required field: CompanyName");
            });
        }
    }
}