using FluentValidation;
using FluentValidation.Results;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.RepairOrders
{
    public static class VehicleValidatorExtensions
    {
        public static bool IsValid(this Vehicle vehicle, out string errors)
        {
            return VehicleValidator.IsValid(vehicle, out errors);
        }
    }

    public class VehicleValidator : AbstractValidator<Vehicle>
    {
        public static bool IsValid(Vehicle vehicle, out string errors)
        {
            var validationResult = new VehicleValidator().Validate(vehicle);
            errors = CreateErrorMessage(validationResult.Errors);
            return validationResult.IsValid;
        }

        private static string CreateErrorMessage(IList<ValidationFailure> validationErrors)
        {
            if (validationErrors.Count == 0)
            {
                return null;
            }
            return "Vehicle is missing the following required fields: " + string.Join(", ", validationErrors.Select(validationError => validationError.PropertyName));
        }
    }
}