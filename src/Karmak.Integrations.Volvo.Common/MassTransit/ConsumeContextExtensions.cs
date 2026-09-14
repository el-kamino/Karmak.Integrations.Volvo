using Karmak.Integrations.Elk.Identity.Context;
using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    public static class ConsumeContextExtensions
    {
        public static ElkContext ExtractImplicitElkContext(this ConsumeContext messageContext)
        {
            return messageContext.Headers.ExtractImplicitElkContext();
        }
    }
}