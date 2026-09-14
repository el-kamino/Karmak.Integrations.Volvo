using System;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    [Serializable]
    public class InvalidClaimContextException : Exception
    {
        private const string DefaultMessage = "Invalid claim context.";
        public InvalidClaimContextException() : base(DefaultMessage) { }
        public InvalidClaimContextException(string message) : base(message) { }
        public InvalidClaimContextException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidClaimContextException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
