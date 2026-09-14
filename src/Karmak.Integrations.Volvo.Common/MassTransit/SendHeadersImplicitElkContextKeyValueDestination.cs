using Karmak.Integrations.Elk.Identity.Extraction;
using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    internal class SendHeadersImplicitElkContextKeyValueDestination : IImplicitElkContextKeyValueDestination<SendHeaders, string, string>
    {
        public static readonly SendHeadersImplicitElkContextKeyValueDestination Instance =
            new SendHeadersImplicitElkContextKeyValueDestination();

        private SendHeadersImplicitElkContextKeyValueDestination()
        {}

        public void SetValue(SendHeaders source, string key, string value)
        {
            source.Set(key, value);
        }
    }
}