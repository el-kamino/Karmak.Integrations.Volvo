using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    public static class HeadersExtensions
    {
        public static ElkContext ExtractImplicitElkContext(this Headers headers)
        {
            return ImplicitElkContext.Extract(
                headers,
                MassTransitHeaderImplicitElkContextKeySpecification.Instance,
                HeadersImplicitElkContextKeyValueSource.Instance);
        }
    }
}