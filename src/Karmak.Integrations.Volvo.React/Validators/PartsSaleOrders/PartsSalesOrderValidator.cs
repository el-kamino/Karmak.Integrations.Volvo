using FluentValidation;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Karmak.Integrations.Volvo.React.Validators.PartsSaleOrders
{
    public class PartsSalesOrderValidator : BaseValidator<PartsSalesOrder>
    {
        public PartsSalesOrderValidator()
        {
            RuleFor(salesOrder => salesOrder.Parts)
                .NotNull()
                .NotEmpty()
                .When(salesOrder => salesOrder.MiscCharges == null || salesOrder.MiscCharges.Count == 0);

            RuleForEach(salesOrder => salesOrder.Parts)
                .SetValidator(new PartsValidator())
                .When(parts => parts != null);

            RuleFor(salesOrder => salesOrder.BillingCustomer)
                .SetValidator(new CustomerValidator());

            RuleFor(salesOrder => salesOrder.ShipToCustomer)
                .SetValidator(new CustomerValidator());
        }
    }

    public static class PartsSalesOrderExtensions
    {
        public static bool IsValid(this PartsSalesOrder partsSalesOrder, out string errors)
        {
            return new PartsSalesOrderValidator().IsValid(partsSalesOrder, out errors);
        }
    }
}