using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    [Serializable]
    public class ReconcilliationFetchException : Exception
    {
        public ReconcilliationFetchException(string message) : base(message)
        {
        }

        public ReconcilliationFetchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
