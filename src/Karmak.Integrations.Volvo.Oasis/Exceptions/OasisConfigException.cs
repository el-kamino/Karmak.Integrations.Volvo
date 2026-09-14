namespace Karmak.Integrations.Volvo.Oasis.Exceptions
{
    [Serializable]
    public class OasisConfigException : Exception
    {
        public OasisConfigException()
        {
        }

        public OasisConfigException(string message) : base(message)
        {
        }
    }
}
