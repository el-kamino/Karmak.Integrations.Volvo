using System;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.React
{
    [Serializable]
    public class VolvoIntegrationServiceException : Exception
    {
        public VolvoIntegrationServiceException()
            : this(false)
        { }

        public VolvoIntegrationServiceException(bool transient)
            : base()
        {
            Transient = transient;
        }

        public VolvoIntegrationServiceException(string message)
            : this(message, false)
        { }

        public VolvoIntegrationServiceException(string message, bool transient)
            : base(message)
        {
            Transient = transient;
        }

        public VolvoIntegrationServiceException(string message, Exception innerException)
            : this(message, false, innerException)
        { }

        public VolvoIntegrationServiceException(string message, bool transient, Exception innerException)
            : base(message, innerException)
        {
            Transient = transient;
        }

        protected VolvoIntegrationServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Transient = info.GetBoolean(nameof(Transient));
        }

        public bool Transient { get; } = false;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info?.AddValue(nameof(Transient), Transient);
            base.GetObjectData(info, context);
        }
    }
}