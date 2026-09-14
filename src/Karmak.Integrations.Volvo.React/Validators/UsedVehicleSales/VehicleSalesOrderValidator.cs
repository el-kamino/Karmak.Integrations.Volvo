using FluentValidation;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using Karmak.Integrations.Volvo.React.Validators;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.UsedVehicleSales
{
    public class VehicleSalesOrderValidator : BaseValidator<VehicleSalesOrder>
    {
        public VehicleSalesOrderValidator()
        {
            RuleFor(order => order.InvoiceNumber)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage("Missing required InvoiceNumber");
            RuleFor(order => order.SalesPersonId)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage("Missing required SalesPersonId");
            RuleFor(order => order.SoldVehicles)
                .NotEmpty();
        }
    }

    public static class VehicleSalesOrderExtensions
    {
        public static bool IsValid(this VehicleSalesOrder order, out string errors)
        {
            return new VehicleSalesOrderValidator().IsValid(order, out errors);
        }

        public static bool HasNoUsedVehicles(this VehicleSalesOrder order)
        {
            return order.SoldVehicles.All(v => v.Condition != VehicleCondition.Used);
        }
    }
}