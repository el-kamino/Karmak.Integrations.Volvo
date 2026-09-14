using Karmak.Integrations.Elk.Identity.Context;
using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    public static class SendContextExtensions
    {
        internal static void AddImplicitElkContext(this SendContext sendContext, ElkContext elkContext)
        {
            sendContext.Headers.AddImplicitElkContext(elkContext);
        }

        public static ElkContext ExtractImplicitElkContext(this SendContext sendContext)
        {
            return sendContext.Headers.ExtractImplicitElkContext();
        }
    }
}