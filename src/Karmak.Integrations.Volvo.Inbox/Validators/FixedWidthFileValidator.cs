using FluentValidation;
using Karmak.Integrations.Volvo.Inbox.Models;

namespace Karmak.Integrations.Volvo.Inbox.Validators;

public class FixedWidthFileValidator : AbstractValidator<FixedWidthFile>
{
    public FixedWidthFileValidator()
    {
        RuleFor(m => m)
            .Must(HaveLineItems)
            .WithMessage("File must have line items");

        RuleFor(m => m.CreatedDate)
            .NotEmpty()
            .WithMessage("CreatedDate is required");
    }
    private static bool HaveLineItems(FixedWidthFile fixedWidthFile)
    {
        if (fixedWidthFile.LineItems == null)
        {
            return false;
        }
        return fixedWidthFile.LineItems.Any();
    }
}