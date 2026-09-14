using MassTransit;
using MassTransit.Configuration;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    internal class ExtractImplicitElkContextFromMessagePipeSpecification<T> : IPipeSpecification<T> where T : class, ConsumeContext
    {
        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new ExtractImplicitElkContextFromMessageFilter<T>());
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
}