using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public class PushValidationHandler : IPushValidationHandler
    {
        private readonly IValidator<UpdateSnapshotMessage> _validator;

        public PushValidationHandler(IValidator<UpdateSnapshotMessage> validator = null)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public IEnumerable<ValidatedUpdateSnapshotMessage> Validate(IEnumerable<UpdateSnapshotMessage> updates)
        {
            var listUpdates = updates.ToList();
            var validatedUpdates = new List<ValidatedUpdateSnapshotMessage>();
            for (int i = 0; i < listUpdates.Count; i++)
            {
                var curr = listUpdates[i];
                var validationResults = _validator.Validate(curr);
                var validationUpdate = new ValidatedUpdateSnapshotMessage
                {
                    Errors = !validationResults.IsValid ? FormatErrorList(validationResults.Errors) : null,
                    UpdateSnapshotMessage = curr
                };
                validatedUpdates.Add(validationUpdate);
            }
            return validatedUpdates;
        }

        private IEnumerable<string> FormatErrorList(IList<FluentValidation.Results.ValidationFailure> errors) =>
            errors.Select(x => x.ErrorMessage);
    }
}

