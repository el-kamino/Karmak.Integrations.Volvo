using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeMeterReading
    {
        private static readonly Faker<MeterReading> OdometerFaker = new Faker<MeterReading>()
            .StrictMode(true)
            .RuleFor(meterReading => meterReading.Type, faker => MeterReadingType.Odometer)
            .RuleFor(meterReading => meterReading.ReadingIn, FakeMeasurement.GenerateMiles)
            .RuleFor(meterReading => meterReading.ReadingOut, FakeMeasurement.GenerateMiles);

        private static readonly Faker<MeterReading> EngineFaker = new Faker<MeterReading>()
            .StrictMode(true)
            .RuleFor(meterReading => meterReading.Type, faker => MeterReadingType.EngineHours)
            .RuleFor(meterReading => meterReading.ReadingIn, FakeMeasurement.GenerateHours)
            .RuleFor(meterReading => meterReading.ReadingOut, FakeMeasurement.GenerateHours);

        public static MeterReading GenerateOdometer() => OdometerFaker.Generate();
        public static MeterReading GenerateEngine() => EngineFaker.Generate();
        public static IEnumerable<MeterReading> Generate() => new List<MeterReading> { GenerateOdometer(), GenerateEngine() };
    }
}
