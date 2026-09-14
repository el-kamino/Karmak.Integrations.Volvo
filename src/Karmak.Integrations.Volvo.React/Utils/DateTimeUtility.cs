using System;

namespace Karmak.Integrations.Volvo.React.Utils
{
    internal class DateTimeUtility
    {
        public static string ApplyCustomTimeZone(DateTime? localDateTime, decimal? timeZone)
        {
            if (localDateTime is null || localDateTime == default(DateTime))
                return null;

            var dt = new DateTime(localDateTime.Value.Ticks, DateTimeKind.Unspecified);
            DateTimeOffset dto = new DateTimeOffset(dt, TimeSpan.FromHours((double)timeZone.Value));
            return dto.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }

        public static string FormatLocalTime(DateTime? localDateTime)
        {
            if (localDateTime is null || localDateTime == default)
                return default;

            var local = DateTime.SpecifyKind(localDateTime.Value, DateTimeKind.Local);
            return local.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }
    }
}
