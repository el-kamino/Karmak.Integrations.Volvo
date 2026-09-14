using FluentValidation;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.React.Validators.PartsInventory
{
    public class PartValidator : AbstractValidator<InventoryPart>
    {
        public PartValidator()
        {
            RuleFor(part => part.PartNumber)
                .Must(identifier => !string.IsNullOrWhiteSpace(identifier))
                .WithMessage("Part Identifier can not be null");

            RuleFor(part => part.DataForCurrentPeriod.QuantityOnHand)
                .NotNull()
                .WithMessage("Quantity on Hand must be available");

            RuleFor(part => part.StockingStatus)
                .NotNull()
                .WithMessage("Stocking Status can not be null");

            RuleFor(part => part.QuantitySoldHistory)
                .NotNull()
                .NotEmpty()
                .WithMessage("Part must have historical Data");

            RuleFor(part => part.QuantitySoldHistory)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleForEach(part => part.QuantitySoldHistory)
                        .SetValidator(new MonthlyDataValidator());
                });
        }
    }
}