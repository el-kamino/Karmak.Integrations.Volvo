using System;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    [Serializable]
    public class OWSClaimSubmissionException : Exception
    {
        private const string DefaultMessage = "OWS Claim Submission Exception";
        public OWSClaimSubmissionException() : base(DefaultMessage) { }
        public OWSClaimSubmissionException(string message) : base(message) { }
        public OWSClaimSubmissionException(string message, string faultType) : base($"OWS Claim Submission Exception [{faultType}] : {message}") { }
        public OWSClaimSubmissionException(string message, Exception innerException) : base(message, innerException) { }
        protected OWSClaimSubmissionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
