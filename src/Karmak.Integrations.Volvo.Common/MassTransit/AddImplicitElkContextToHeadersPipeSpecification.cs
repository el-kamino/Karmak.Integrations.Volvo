using MassTransit;
using MassTransit.Configuration;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    internal class AddImplicitElkContextToHeadersPipeSpecification<T> : IPipeSpecification<T> where T : class, SendContext
    {
        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new AddImplicitElkContextToHeadersFilter<T>());
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return Array.Empty<ValidationResult>();
        }
    }
}