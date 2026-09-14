namespace Karmak.Integrations.Volvo.React.Utils
{
    public class BenchmarkResult
    {
        public ElapsedTimeInMilliseconds ElapsedTimeInMilliseconds { get; internal set; }
    }

    public class BenchmarkResult<TResult> : BenchmarkResult
    {
        public TResult Result { get; internal set; }
    }
}