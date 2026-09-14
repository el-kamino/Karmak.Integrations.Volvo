using Karmak.Integrations.Elk.Identity;
using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    internal class ExtractImplicitElkContextFromMessageFilter<T> : IFilter<T> where T : class, ConsumeContext
    {
        public async Task Send(T context, IPipe<T> next)
        {
            var elkContext = context.ExtractImplicitElkContext();
            await ImplicitElkContext.WithCurrentAsync(elkContext, async () =>
            {
                await next.Send(context);
            });
        }

        public void Probe(ProbeContext context)
        {}
    }
}