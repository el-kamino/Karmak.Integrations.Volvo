using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    internal static class SendHeadersExtensions
    {
        /*
         * Note:
         * SendHeaders does not need an ExtractImplicitElkContext extension because
         * it extends the Headers interface and can lean on those extensions.
         */
        internal static void AddImplicitElkContext(this SendHeaders headers, ElkContext elkContext)
        {
            ImplicitElkContext.Add(
                headers,
                elkContext,
                MassTransitHeaderImplicitElkContextKeySpecification.Instance,
                SendHeadersImplicitElkContextKeyValueDestination.Instance);
        }
    }
}