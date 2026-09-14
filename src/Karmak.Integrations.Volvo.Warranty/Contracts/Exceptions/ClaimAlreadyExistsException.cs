// ReSharper disable UnusedMember.Global

using System;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    [Serializable]
    public class ClaimAlreadyExistsException : Exception
    {
        private const string DefaultMessage = "An active claim already exists matching the criteria.";
        public ClaimAlreadyExistsException() : base(DefaultMessage) { }
        public ClaimAlreadyExistsException(string message) : base(message) { }
        public ClaimAlreadyExistsException(string message, Exception innerException) : base(message, innerException) { }
        protected ClaimAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
