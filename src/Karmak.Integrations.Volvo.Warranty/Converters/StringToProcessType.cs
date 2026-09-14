using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class StringToProcessType : IConvertible<string, ProcessType>
    {
        public ProcessType Convert(string source) =>
            Conversions.GetOrNull(source, s => new ProcessType
            {
                acknowledgeCode = s
            });
    }
}
