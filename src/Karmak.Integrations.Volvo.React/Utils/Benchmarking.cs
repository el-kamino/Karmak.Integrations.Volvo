using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class Benchmarking
    {
        public static BenchmarkResult<T> Measure<T>(Func<T> f)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var value = f();
                stopwatch.Stop();
                return new BenchmarkResult<T>
                {
                    ElapsedTimeInMilliseconds = new ElapsedTimeInMilliseconds(stopwatch.ElapsedMilliseconds),
                    Result = value
                };
            }
            finally
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
            }
        }

        public static async Task<BenchmarkResult> Measure(Func<Task> f)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await f();
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    ElapsedTimeInMilliseconds = new ElapsedTimeInMilliseconds(stopwatch.ElapsedMilliseconds)
                };
            }
            finally
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
            }
        }

        public static async Task<BenchmarkResult<T>> Measure<T>(Func<Task<T>> f)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var value = await f();
                stopwatch.Stop();
                return new BenchmarkResult<T>
                {
                    ElapsedTimeInMilliseconds = new ElapsedTimeInMilliseconds(stopwatch.ElapsedMilliseconds),
                    Result = value
                };
            }
            finally
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
            }
        }
    }
}