namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class MeterReading
    {
        public MeterReadingType Type { get; set; }
        public Measurement ReadingIn { get; set; }
        public Measurement ReadingOut { get; set; }

        public static MeterReading CreateWithNullReadings(MeterReadingType type) => new MeterReading
        {
            Type = type,
            ReadingIn = Measurement.Null,
            ReadingOut = Measurement.Null
        };
    }
}
