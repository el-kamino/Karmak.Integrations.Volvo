using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class ClaimTypeToJobType : IConvertible<IClaim, string>
    {
        public string Convert(IClaim source) =>
            Conversions.GetOrNull(source, s => !string.IsNullOrEmpty(s.Type), s => s.InAppeal ? "A1" : s.Type);
    }
}
