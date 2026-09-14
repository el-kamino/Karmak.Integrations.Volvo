using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class StringToCodeTypes : IConvertible<IEnumerable<string>, CodeType[]>
    {
        public CodeType[] Convert(IEnumerable<string> source) =>
            Conversions.GetOrNull(source, s => s
                .Select(value => Conversions.StringToCodeType.Convert(value))
                .Where(value => value != null)
                .ToArray());
    }
}
