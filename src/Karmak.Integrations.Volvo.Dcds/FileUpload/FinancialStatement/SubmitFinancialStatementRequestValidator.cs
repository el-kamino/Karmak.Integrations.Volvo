using FluentValidation;
using Karmak.Integrations.Volvo.Dcds.Contracts;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.FinancialStatement
{
    public class SubmitFinancialStatementRequestValidator : AbstractValidator<FinancialStatementRequest>
    {
        public SubmitFinancialStatementRequestValidator()
        {
            RuleFor(m => m.PACode)
                .NotEmpty()
                .WithMessage(ValidationConstants.REQUIRED_FIELD);
            
            RuleFor(m => m.FormattedFileContents)
                .NotEmpty()
                .WithMessage(ValidationConstants.REQUIRED_FIELD);
        }
    }
}