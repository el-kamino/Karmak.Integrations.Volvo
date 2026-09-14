namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class VinFormatter
    {
        private const int VIN_LENGTH = 17;

        public static string Format(string vin)
        {
            vin = string.IsNullOrWhiteSpace(vin) ? "UNKNOWN" : vin;
            return vin.PadLeft(VIN_LENGTH, '0').MaxLength(VIN_LENGTH);
        }
    }
}
