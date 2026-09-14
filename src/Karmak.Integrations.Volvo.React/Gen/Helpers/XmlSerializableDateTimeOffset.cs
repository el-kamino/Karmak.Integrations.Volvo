using System;

namespace Karmak.Integrations.Volvo.React.Gen.Helpers
{
    public class XmlSerializableDateTimeOffset : System.Xml.Serialization.IXmlSerializable
    {
        private static readonly string DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";

        DateTimeOffset _dto;

        private XmlSerializableDateTimeOffset() { _dto = default; }
        private XmlSerializableDateTimeOffset(DateTimeOffset dto) { _dto = dto; }
        private XmlSerializableDateTimeOffset(DateTime dt, TimeSpan ts) : this(new DateTimeOffset(dt, ts)) { }
        private XmlSerializableDateTimeOffset(DateTime dt, decimal? tz = null) : this(dt, GetTimeSpan(dt, tz)) { }

        private static TimeSpan GetTimeSpan(DateTime dt, decimal? tz) =>
            new TimeSpan((int)(tz ?? TimeZoneInfo.Local.GetUtcOffset(dt).Hours), 0, 0);

        internal static XmlSerializableDateTimeOffset GetFormattedDateTimeOffset(DateTime? date, decimal? timeZone)
        {
            if (date is null || date == default(DateTime))
                return new XmlSerializableDateTimeOffset();

            // must be unspecified to use custom time span
            var ticks = date.Value.ToLocalTime().Ticks;
            var dt = new DateTime(ticks - ticks % TimeSpan.TicksPerSecond, DateTimeKind.Unspecified);
            return new XmlSerializableDateTimeOffset(dt, timeZone);
        }

        public System.Xml.Schema.XmlSchema GetSchema() => null;
        public void ReadXml(System.Xml.XmlReader reader) => _dto = DateTimeOffset.Parse(reader.ReadElementContentAsString());
        public void WriteXml(System.Xml.XmlWriter writer) => writer.WriteString(_dto.ToString(DateTimeFormat));
    }
}
