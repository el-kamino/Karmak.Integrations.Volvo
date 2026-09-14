namespace Karmak.Integrations.Volvo.React.Utils
{
    public class ElapsedTimeInMilliseconds
    {
        private readonly long _value;

        public ElapsedTimeInMilliseconds(long value)
        {
            _value = value;
        }

        public static implicit operator string(ElapsedTimeInMilliseconds elapsedTimeInMilliseconds)
        {
            return elapsedTimeInMilliseconds._value.ToString();
        }

        public static implicit operator long(ElapsedTimeInMilliseconds elapsedTimeInMilliseconds)
        {
            return elapsedTimeInMilliseconds._value;
        }
    }
}