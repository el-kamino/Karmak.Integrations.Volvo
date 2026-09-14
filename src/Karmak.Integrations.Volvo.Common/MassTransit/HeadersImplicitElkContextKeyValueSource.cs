using Karmak.Integrations.Elk.Identity.Extraction;
using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    internal class HeadersImplicitElkContextKeyValueSource : IImplicitElkContextKeyValueSource<Headers, string, string>
    {
        public static readonly HeadersImplicitElkContextKeyValueSource Instance =
            new HeadersImplicitElkContextKeyValueSource();

        private HeadersImplicitElkContextKeyValueSource()
        { }

        public string GetValue(Headers source, string key)
        {
            return source.Get<string>(key);
        }
    }
}