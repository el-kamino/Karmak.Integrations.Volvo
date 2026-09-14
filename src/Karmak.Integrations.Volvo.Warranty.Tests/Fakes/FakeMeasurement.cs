using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeMeasurement
    {
        private static readonly Faker<Measurement> MilesFaker = new Faker<Measurement>()
            .StrictMode(true)
            .RuleFor(measurement => measurement.UnitOfMeasure, faker => UnitOfMeasureType.Miles)
            .RuleFor(measurement => measurement.Value, faker => faker.Random.Number());

        private static readonly Faker<Measurement> HoursFaker = new Faker<Measurement>()
            .StrictMode(true)
            .RuleFor(measurement => measurement.UnitOfMeasure, faker => UnitOfMeasureType.Hours)
            .RuleFor(measurement => measurement.Value, faker => faker.Random.Number());

        public static Measurement GenerateMiles() => MilesFaker.Generate();
        public static Measurement GenerateHours() => HoursFaker.Generate();
    }
}
