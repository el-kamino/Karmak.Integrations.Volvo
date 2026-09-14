using FluentValidation;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;

namespace Karmak.Integrations.Volvo.Inbox.Validators;

public class InboxMessageUpdateArgumentsValidator : AbstractValidator<InboxMessageUpdateArguments>
{
    public InboxMessageUpdateArgumentsValidator()
    {
        RuleFor(m => m.Id)
            .NotEmpty()
            .WithMessage("Required");

        RuleFor(m => m)
            .Custom((m, ctx) =>
            {
                if (m.IsActive == null && m.IsPrinted == null && m.IsRead == null)
                    ctx.AddFailure("There is nothing to update");
            });
    }
}