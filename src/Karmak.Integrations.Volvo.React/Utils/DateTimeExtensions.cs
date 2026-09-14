using System;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTime WithoutMilliseconds(this DateTime date)
        {
            return new DateTime(date.Ticks - date.Ticks % TimeSpan.TicksPerSecond, date.Kind);
        }

        public static string InTimestampFormat(this DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        public static DateTime EndOfDay(this DateTime date) {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }
    }
}