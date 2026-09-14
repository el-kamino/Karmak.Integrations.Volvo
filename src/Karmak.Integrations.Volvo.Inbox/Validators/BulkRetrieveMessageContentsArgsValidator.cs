using FluentValidation;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;

namespace Karmak.Integrations.Volvo.Inbox.Validators;

public class BulkRetrieveMessageContentsArgsValidator : AbstractValidator<BulkRetrieveMessageContentsArgs>
{
    public BulkRetrieveMessageContentsArgsValidator()
    {
        RuleFor(m => m.MessageIds)
            .NotEmpty()
            .WithMessage("At least one message id must be included");

        RuleForEach(m => m.MessageIds)
            .NotEmpty()
            .WithMessage("Null MessageIds cannot be included in the request");
    }
}