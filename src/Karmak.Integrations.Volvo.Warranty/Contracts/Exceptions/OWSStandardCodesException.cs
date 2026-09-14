using System;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    [Serializable]
    public class OWSStandardCodesException : Exception
    {
        private const string DefaultMessage = "OWS Standard Codes Exception";
        public OWSStandardCodesException() : base(DefaultMessage) { }
        public OWSStandardCodesException(string message) : base(message) { }
        public OWSStandardCodesException(string message, string faultType) : base($"OWS Retrieval Exception [{faultType}] : {message}") { }
        public OWSStandardCodesException(string message, Exception innerException) : base(message, innerException) { }
        protected OWSStandardCodesException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
