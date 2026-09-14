using Karmak.Integrations.Volvo.React.Contracts.Common;
using FluentValidation;
using Karmak.Integrations.Volvo.React.Validators;

namespace Karmak.Integrations.Volvo.React.Validators.CustomerUpdate
{
    public class AddressValidator : BaseValidator<Address> {
        public AddressValidator() {
            RuleFor(a => a.City)
                .Must(s => !string.IsNullOrWhiteSpace(s));
        }

        public static bool IsValid(Address address) {
            return address != null && new AddressValidator().Validate(address).IsValid;
        }
    }
}