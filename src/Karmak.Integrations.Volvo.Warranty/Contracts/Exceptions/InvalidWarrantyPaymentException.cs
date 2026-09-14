using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    public class InvalidWarrantyPaymentException : Exception
    {
        private const string DefaultMessage = "Invalid payment information.";

        public InvalidWarrantyPaymentException() : base(DefaultMessage) { }
        public InvalidWarrantyPaymentException(string message) : base(message) { }
        public InvalidWarrantyPaymentException(string message, Exception innerException) : base(message, innerException) { }
    }
}
