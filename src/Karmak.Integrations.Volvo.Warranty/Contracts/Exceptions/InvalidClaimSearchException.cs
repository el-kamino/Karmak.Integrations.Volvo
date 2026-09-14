using System;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    /// <summary>
    /// A claim search that cannot be run as asked: a field or operator that does not exist, a value
    /// that is not of the field's type, or a request that is neither one mode nor the other. The
    /// caller is the one who can fix it, so it surfaces as a bad request rather than a failure.
    /// </summary>
    [Serializable]
    public class InvalidClaimSearchException : Exception
    {
        private const string DefaultMessage = "Invalid claim search.";
        public InvalidClaimSearchException() : base(DefaultMessage) { }
        public InvalidClaimSearchException(string message) : base(message) { }
        public InvalidClaimSearchException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidClaimSearchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
