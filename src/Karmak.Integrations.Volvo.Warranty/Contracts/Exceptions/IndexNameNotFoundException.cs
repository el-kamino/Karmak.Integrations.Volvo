using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    public class IndexNameNotFoundException : Exception
    {
        public IndexNameNotFoundException(string message) : base(message)
        {
        }

        public IndexNameNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IndexNameNotFoundException()
        {
        }
    }
}
