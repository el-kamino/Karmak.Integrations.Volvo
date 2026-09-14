using Karmak.Integrations.Elk.Identity;
using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    internal class AddImplicitElkContextToHeadersFilter<T> : IFilter<T> where T : class, SendContext
    {
        public Task Send(T context, IPipe<T> next)
        {
            context.AddImplicitElkContext(ImplicitElkContext.Current);
            return Task.CompletedTask;
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}