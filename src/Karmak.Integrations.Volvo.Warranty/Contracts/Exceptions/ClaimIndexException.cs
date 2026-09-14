using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    public class ClaimIndexException : Exception
    {
        public ClaimIndexException(string message) : base(message)
        {
        }

        public ClaimIndexException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ClaimIndexException()
        {
        }
    }
}
