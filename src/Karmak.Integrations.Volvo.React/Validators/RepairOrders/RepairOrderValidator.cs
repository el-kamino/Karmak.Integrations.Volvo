using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.RepairOrders.Extensions;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class RepairOrderExtensions
    {
        public static bool IsValid(this RepairOrderSnapshot repairOrder, out string errors)
        {
            return RepairOrderValidator.IsValid(repairOrder, out errors);
        }
    }

    public class RepairOrderValidator : AbstractValidator<RepairOrderSnapshot>
    {
        private RepairOrderValidator()
        {
            RuleFor(repairOrder => repairOrder.DealerInfo)
                .NotNull();

            RuleForEach(repairOrder => repairOrder.Tasks)
                .SetValidator(new RepairOrderTaskValidator())
                .When(repairOrder => repairOrder.Tasks != null);

            RuleFor(repairOrder => repairOrder.RepairOrderNumber)
                .NotNull();

//          Electing to handle poorly formed OwningCustomer at mapper by omitting just the customer.
//            RuleFor(repairOrder => repairOrder.OwningCustomer)
//                .NotNull()
//                .SetValidator(new CustomerValidator());

//          Electing to handle poorly formed Driver at mapper by omitting just the driver.
//            RuleFor(repairOrder => repairOrder.Driver)
//                .SetValidator(new ContactValidator());

            RuleFor(repairOrder => repairOrder.DatabaseVersion)
                .Must(s => !string.IsNullOrWhiteSpace(s));

            RuleFor(repairOrder => repairOrder.Vehicle)
                .NotNull()
                .SetValidator(new VehicleValidator());

            RuleFor(repairOrder => repairOrder.ServiceWriterName)
                .Must(s => !string.IsNullOrWhiteSpace(s));

            RuleFor(repairOrder => repairOrder.RepairOrderStatus)
                .Must(s => !string.IsNullOrWhiteSpace(s));

            RuleFor(repairOrder => repairOrder.OpenDate)
                .NotNull();

            RuleFor(repairOrder => repairOrder.GetOwningCustomerAddress())
                .NotNull()
                .SetValidator(new AddressValidator());
        }

        public static bool IsValid(RepairOrderSnapshot repairOrder, out string errors)
        {
            var validationResult = new RepairOrderValidator().Validate(repairOrder);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "RepairOrderDto is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}