using System;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    [Serializable]
    public class ClaimNotFoundException : Exception
    {
        private const string DefaultMessage = "A claim could not be found based on the criteria.";

        public ClaimNotFoundException() : base(DefaultMessage) { }
        public ClaimNotFoundException(string message) : base(message) { }
        public ClaimNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        protected ClaimNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
