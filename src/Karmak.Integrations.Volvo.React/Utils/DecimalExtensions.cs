using System;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class DecimalExtensions
    {
        public static decimal WithDecimalImplied(this decimal input, int numberOfDecimalsImplied = 2, Func<decimal,decimal> reductionMethod = null)
        {
            var multiplier = (decimal)Math.Pow(10, numberOfDecimalsImplied);
            if (reductionMethod == null)
                return Math.Truncate(multiplier * input);
            return reductionMethod(multiplier * input);
        }

        public static decimal? OrMax(this decimal? input, decimal max)
        {
            return input.HasValue
                ? Math.Min(input.Value, max)
                : input;
        }

        public static decimal OrMax(this decimal input, decimal max)
        {
            return Math.Min(input, max);
        }

        public static decimal Truncate(this decimal input)
        {
            return Math.Truncate(input);
        }

        public static decimal Round(this decimal input)
        {
            return Math.Round(input);
        }
        public static decimal IsCanceled(this decimal input, bool isCanceled)
        {
            if (isCanceled) return 0;
            return input;
        }
    }
}