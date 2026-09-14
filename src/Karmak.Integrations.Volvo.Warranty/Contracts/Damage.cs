namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Damage
    {
        public string Area { get; set; }
        public string Type { get; set; }
        public string Severity { get; set; }
        public string Code => Area + Type + Severity;
    }
}
