using System;

namespace Karmak.Integrations.Volvo.Warranty.Utilities
{
    public class DateTimeProvider : IDateTimeProvider, IDateTimeOffsetProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        DateTimeOffset IDateTimeOffsetProvider.Now => DateTimeOffset.Now;

        public DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
