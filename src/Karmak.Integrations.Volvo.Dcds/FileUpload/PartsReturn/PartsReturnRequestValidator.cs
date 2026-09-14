using FluentValidation;
using Karmak.Integrations.Volvo.Dcds.Contracts;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload.PartsReturn
{
    public class PartsReturnRequestValidator : AbstractValidator<PartsReturnRequest>
    {
        public PartsReturnRequestValidator()
        {
            RuleFor(m => m.PACode)
                .NotEmpty()
                .WithMessage(ValidationConstants.REQUIRED_FIELD)
                .MaximumLength(5)
                .WithMessage(ValidationConstants.MAX_LENGTH_5);

            RuleFor(m => m.OrderNumber)
                .NotEmpty()
                .WithMessage(ValidationConstants.REQUIRED_FIELD);

            RuleFor(m => m.RequestDate)
                .NotEmpty()
                .WithMessage(ValidationConstants.REQUIRED_FIELD);

            RuleFor(m => m.DistributionCode)
                .NotEmpty()
                .WithMessage(ValidationConstants.REQUIRED_FIELD);

            RuleFor(m => m.VendorId)
                .NotEmpty()
                .WithMessage(ValidationConstants.REQUIRED_FIELD);

            RuleFor(m => m.PartsToReturn)
                .NotEmpty()
                .WithMessage(ValidationConstants.REQUIRED_FIELD);

            var returnPartItemValidator = new ReturnPartItemValidator();
            RuleForEach(m => m.PartsToReturn)
                .SetValidator(returnPartItemValidator);
        }

        public class ReturnPartItemValidator : AbstractValidator<ReturnPartItem>
        {
            private static readonly List<string> ValidReturnTypes = new List<string> {"PIPP", "POPP", "SP01", "SP02", "SP03"};

            public ReturnPartItemValidator()
            {
                RuleFor(m => m.ReturnType)
                    .Custom((val, ctx) =>
                    {
                        if (string.IsNullOrEmpty(val))
                            ctx.AddFailure(ValidationConstants.REQUIRED_FIELD);

                        else if (!ValidReturnTypes.Contains(val))
                            ctx.AddFailure($"Field must be one of the following type codes: ({string.Join(',', ValidReturnTypes)})");
                    });

                RuleFor(m => m.PartNumber)
                    .NotEmpty()
                    .WithMessage(ValidationConstants.REQUIRED_FIELD)
                    .MaximumLength(18)
                    .WithMessage(ValidationConstants.MAX_LENGTH_18);

                RuleFor(m => m.QuantityToReturn)
                    .NotEmpty()
                    .WithMessage(ValidationConstants.REQUIRED_FIELD);

                RuleFor(m => m.BinNumber)
                    .MaximumLength(7)
                    .WithMessage(ValidationConstants.MAX_LENGTH_7);
            }
        }
    }
}