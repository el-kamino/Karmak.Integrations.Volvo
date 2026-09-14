using System;
using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    [Serializable]
    public class ConversionException<TSource, TDestintation> : Exception
    {
        private const string InvalidCastExceptionTemplate = "Cannot cast value '{0}' of type '{1}' to '{2}'";

        public ConversionException(TSource value)
            : base(string.Format(InvalidCastExceptionTemplate, value, typeof(TSource), typeof(TDestintation))) { }

        protected ConversionException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
