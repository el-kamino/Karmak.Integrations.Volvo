using System;

namespace Karmak.Integrations.Volvo.Common.Bridge
{
    [Serializable]
    public class RelayCommunicationException : Exception
    {
        public RelayCommunicationException()
        {
        }

        public RelayCommunicationException(string message) : base(message)
        {
        }

        public RelayCommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}