using FluentValidation;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Karmak.Integrations.Volvo.React.Validators.PartsSaleOrders
{
    public class PartsValidator : AbstractValidator<Part>
    {
        public PartsValidator()
        {
            RuleFor(part => part.Description)
                .Must(description => !string.IsNullOrWhiteSpace(description))
                .WithMessage("Part Description is missing");

            RuleFor(part => part.Number)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Part Number is missing");
        }
    }
}