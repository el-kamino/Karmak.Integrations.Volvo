using FluentValidation;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;

namespace Karmak.Integrations.Volvo.Inbox.Validators;

public class PagingArgumentsValidator : AbstractValidator<PagingArguments>
{
    public PagingArgumentsValidator()
    {
        RuleFor(m => m.PageSize)
            .NotEmpty()
            .WithMessage("Must be greater than 0")
            .GreaterThan(0)
            .WithMessage("Must be greater than 0");

        RuleFor(m => m.PageNumber)
            .GreaterThan(0)
            .WithMessage("Must be greater than 0");
    }
}