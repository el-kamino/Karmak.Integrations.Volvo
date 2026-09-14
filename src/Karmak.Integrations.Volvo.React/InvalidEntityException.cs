using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.React
{
    [Serializable]
    public class InvalidEntityException : VolvoIntegrationServiceException {
        public Dictionary<string, string> Metadata { get; }
        public string EntityType { get; }

        public InvalidEntityException(Type entityType, string errorMessage, Dictionary<string, string> metadata) : base(errorMessage) {
            Metadata = metadata;
            EntityType = entityType.ToString();
        }

        protected InvalidEntityException(SerializationInfo info, StreamingContext context) : base(info, context) {
            EntityType = info.GetString(nameof(EntityType));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info?.AddValue(nameof(EntityType), EntityType);
            base.GetObjectData(info, context);
        }
    }
}