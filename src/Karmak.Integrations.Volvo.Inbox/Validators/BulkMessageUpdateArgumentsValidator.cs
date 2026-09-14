using FluentValidation;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;

namespace Karmak.Integrations.Volvo.Inbox.Validators;

public class BulkMessageUpdateArgumentsValidator : AbstractValidator<BulkMessageUpdateArguments>
{
    public BulkMessageUpdateArgumentsValidator()
    {
        RuleFor(m => m.Messages)
            .NotEmpty()
            .WithMessage("At least one message must be included for update");

        var messageValidator = new InboxMessageUpdateArgumentsValidator();
        RuleForEach(m => m.Messages)
            .SetValidator(messageValidator);
    }
}