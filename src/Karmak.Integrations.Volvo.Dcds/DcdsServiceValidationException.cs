namespace Karmak.Integrations.Volvo.Dcds
{
    public class DcdsServiceValidationException : Exception
    {
        public DcdsServiceValidationException(string message) 
            : base(message)
        {
        }
    }
}