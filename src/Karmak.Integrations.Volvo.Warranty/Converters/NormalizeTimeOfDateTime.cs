using System;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class NormalizeTimeOfDateTime : IConvertible<DateTime?, DateTime?>
    {
        public DateTime? Convert(DateTime? source)
        {
            if (source == null)
            {
                return null;
            }
            var value = source.Value;
            return new DateTime(value.Year, value.Month, value.Day, 12, 0, 0, DateTimeKind.Utc);
        }
    }
}
