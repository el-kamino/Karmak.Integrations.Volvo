namespace Karmak.Integrations.Volvo.Fusion
{
    public class UnrecoverableFusionRequestException : Exception 
    {
        public UnrecoverableFusionRequestException(Exception e) 
            : base("Fusion request failed in Fusion Adapter. Support intervention required.", e) 
        {
        }
    }
}