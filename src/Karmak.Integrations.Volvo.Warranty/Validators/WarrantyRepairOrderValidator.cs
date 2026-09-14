using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.Warranty.Validators
{
    public class WarrantyRepairOrderValidator : AbstractValidator<RepairOrderSnapshot>
    {
        public const string WarrantyCustomersContextKey = "WarrantyCustomers";
        public WarrantyRepairOrderValidator()
        {
            RuleFor(repairOrder => repairOrder.RepairOrderNumber)
                .NotNull()
                .NotEmpty();
            RuleFor(repairOrder => repairOrder.InvoiceDate).NotNull();
            RuleFor(repairOrder => repairOrder.BillingCustomer)
                .Custom((billingCustomer, context) =>
                {
                    if (billingCustomer == null || !GetWarrantyCustomers(context).Contains(billingCustomer.CustomerKey))
                    {
                        context.AddFailure("BillingCustomer is not configured as a WarrantyCustomer");
                    }
                });
        }

        private static IEnumerable<string> GetWarrantyCustomers(ValidationContext<RepairOrderSnapshot> context)
        {
            string[] warrantyCustomers = { };

            if (context.RootContextData.ContainsKey(WarrantyCustomersContextKey))
            {
                warrantyCustomers = context.RootContextData[WarrantyCustomersContextKey] as string[];
            }

            return warrantyCustomers ?? Array.Empty<string>();
        }
    }
}
