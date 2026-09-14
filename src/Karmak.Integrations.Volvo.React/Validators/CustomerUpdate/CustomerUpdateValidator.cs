using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions;
using CustomerInfo = Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate;

namespace Karmak.Integrations.Volvo.React.Validators.CustomerUpdate
{
    public class CustomerUpdateValidator : AbstractValidator<CustomerInfo>
    {
        private CustomerUpdateValidator()
        {
            RuleFor(ci => ci.DealerInfo)
                .NotNull();
            When(ci => !ci.HasVolvoPassRewards(), () =>
            {
                RuleFor(ci => ci.CurrentVINs)
                    .NotNull()
                    .NotEmpty()
                    .When(ci => !(ci.HasAddedVIN() || ci.HasRemovedVIN()));
                RuleFor(ci => ci.AddedVIN)
                    .Must(s => !string.IsNullOrWhiteSpace(s))
                    .When(ci => !(ci.HasRemovedVIN() || ci.CurrentVINsIsSet()));
                RuleFor(ci => ci.RemovedVIN)
                    .Must(s => !string.IsNullOrWhiteSpace(s))
                    .When(ci => !(ci.HasAddedVIN() || ci.CurrentVINsIsSet()));
            });
            When(ci=>ci.HasNoVin(), () =>
            {
                RuleFor(ci => ci.VolvoPassRewardID)
                    .Must(id => id > 0);
            });
            RuleFor(ci => ci.Customer)
                .SetValidator(new CustomerValidator());
        }

        public static bool IsValid(CustomerInfo record, out string errMsg)
        {
            var validator = new CustomerUpdateValidator();
            var validationResult = validator.Validate(record);
            errMsg = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<FluentValidation.Results.ValidationFailure> errors)
        {
            if (errors.Count == 0)
            {
                return null;
            }
            return "CustomerUpdateDto is missing the following required fields: " + string.Join(", ", errors.Select(validationError => validationError.PropertyName));
        }
    }
}
