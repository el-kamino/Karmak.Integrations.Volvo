using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace Karmak.Integrations.Volvo.React.Validators
{
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        private string CreateErrorMessage(ICollection<ValidationFailure> errors)
        {
            if (errors.Count == 0)
            {
                return null;
            }
            return string.Join(", ", errors.Select(error => error.ErrorMessage));
        }

        public bool IsValid(T thing, out string errors)
        {
            var result = Validate(thing);
            errors = CreateErrorMessage(result.Errors);
            return result.IsValid;
        }
    }
}