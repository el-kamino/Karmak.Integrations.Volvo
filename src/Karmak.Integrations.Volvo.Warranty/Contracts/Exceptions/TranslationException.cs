using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    [Serializable]
    public class TranslationException : Exception
    {
        public TranslationException() { }
        public TranslationException(string message) : base(message) { }
        public TranslationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
