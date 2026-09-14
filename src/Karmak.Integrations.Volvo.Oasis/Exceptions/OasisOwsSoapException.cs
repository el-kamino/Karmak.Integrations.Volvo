namespace Karmak.Integrations.Volvo.Oasis.Exceptions
{
    public class OasisOwsSoapException : Exception
    {
        public OasisOwsSoapException(string message, string faultType) : base($"OWS Retrieval Exception [{faultType}]: {message}")
        {
        }
    }
}
