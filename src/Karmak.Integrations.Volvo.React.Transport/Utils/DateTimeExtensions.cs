using System;

namespace Karmak.Integrations.Volvo.React.Transport.Utils
{
    public static class DateTimeExtensions {
        public static string InTimestampFormat(this DateTime date) {
            return date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
    }
}