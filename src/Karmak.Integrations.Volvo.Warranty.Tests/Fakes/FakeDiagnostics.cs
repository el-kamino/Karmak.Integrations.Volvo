using Bogus;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Fakes
{
    public static class FakeDiagnostics
    {
        private static readonly Faker<Diagnostics> Faker = new Faker<Diagnostics>()
            .StrictMode(true)
            .RuleFor(d => d.IsCheckEngineLightOn, faker => faker.Random.Bool())
            .RuleFor(d => d.PolicyRequiredMeasurementOrResults, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.DiagnosticTroubleCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.BatteryCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.BodyCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.ChassisCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.UndefinedDiagnosticCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.KeyOnEngineOffCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.KeyOnEngineColdCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.KeyOnEngineRunningCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.ReplacedTireDepartmentOfTransportationCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)))
            .RuleFor(d => d.ReplacementTireDepartmentOfTransportationCodes, faker => faker.Random.WordsArray(faker.Random.Int(1, 6)));

        public static Diagnostics Generate() => Faker.Generate();
        public static IEnumerable<Diagnostics> Generate(int count) => Faker.Generate(count);
    }
}
