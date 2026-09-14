using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class NormalizeTimeOfDateTimeTest
    {
        [Fact]
        public void NormalizeTimeOfDateTime_ValidDateSuccess()
        {
            var stubDate = DateTime.Parse("1970-12-31T23:30:30+0:00");
            var normalizer = new NormalizeTimeOfDateTime();

            var result = normalizer.Convert(stubDate);

            Assert.True(result.HasValue);
            var value = result.Value;
            Assert.Equal(1970, value.Year);
            Assert.Equal(12, value.Month);
            Assert.Equal(31, value.Day);
            Assert.Equal(12, value.Hour);
            Assert.Equal(0, value.Minute);
            Assert.Equal(0, value.Second);
        }

        [Fact]
        public void NormalizeTimeOfDateTime_NullDateSuccess()
        {
            var stubDate = new DateTime?();
            var normalizer = new NormalizeTimeOfDateTime();

            var result = normalizer.Convert(stubDate);

            Assert.Null(result);
        }
    }
}
