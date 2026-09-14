using System;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class EnumToString : IConvertible<Enum, string>
    {
        public string Convert(Enum source) => source.ToString("G");
    }
}
