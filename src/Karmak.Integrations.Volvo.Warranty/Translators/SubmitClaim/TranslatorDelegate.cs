using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;

namespace Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim
{
    public class TranslatorDelegate<TArguments, TDestination> : ITranslatable<TArguments, TDestination> where TDestination : class
    {
        private readonly IDictionary<Func<TArguments, bool>, ITranslatable<TArguments, TDestination>> _translators;

        public TranslatorDelegate(IDictionary<Func<TArguments, bool>, ITranslatable<TArguments, TDestination>> translators)
        {
            _translators = translators;
        }

        public TDestination Translate(TArguments args) => _translators
            .AsEnumerable()
            .FirstOrDefault(pair => pair.Key.Invoke(args))
            .Value
            ?.Translate(args) ?? throw new TranslationException("Matching translator not found");
    }
}
