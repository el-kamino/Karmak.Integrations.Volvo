namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Measurement
    {
        public static readonly Measurement Null = new Measurement { UnitOfMeasure = UnitOfMeasureType.None, Value = 0 };
        public UnitOfMeasureType UnitOfMeasure { get; set; }
        public decimal Value { get; set; }
    }
}
