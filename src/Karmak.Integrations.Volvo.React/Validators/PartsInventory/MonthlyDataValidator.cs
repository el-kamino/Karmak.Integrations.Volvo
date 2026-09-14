using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using FluentValidation;

namespace Karmak.Integrations.Volvo.React.Validators.PartsInventory
{
    public class MonthlyDataValidator : AbstractValidator<QuantitySoldRecord>
    {
        public MonthlyDataValidator()
        {
            RuleFor(data => data.Period)
                .NotNull();

            RuleFor(data => data.QuantitySold)
                .NotNull();
        }
    }
}