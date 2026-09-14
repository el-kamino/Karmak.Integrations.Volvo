using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions
{
    public sealed class FusionRpcException : Exception
    {
        public FusionRpcException() : base() { }
        public FusionRpcException(string message) : base(message) { }
        public FusionRpcException(string message, Exception inner) : base(message, inner) { }
    }
}
