using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Translators;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators
{
    public class TranslatorDelegateTest
    {
        [Fact]
        public void WhenNoTranslatorsAreProvided_It_ThrowsAnError()
        {
            var translator = new TranslatorDelegate<string, string>(
                new Dictionary<Func<string, bool>, ITranslatable<string, string>>());

            Assert.Throws<TranslationException>(() => translator.Translate("foo"));
        }

        [Fact]
        public void WhenNoMatchingTranslatorsCanBeFound_It_ThrowsAnError()
        {
            var translator = new TranslatorDelegate<string, string>(
                new Dictionary<Func<string, bool>, ITranslatable<string, string>> {
                    { (source) => false, new StringTranslator("bar") }
                });

            Assert.Throws<TranslationException>(() => translator.Translate("foo"));
        }

        [Fact]
        public void WhenMatchingTranslatorIsFound_It_Translates()
        {
            var translator = new TranslatorDelegate<string, string>(
                new Dictionary<Func<string, bool>, ITranslatable<string, string>> {
                    { (source) => false, new StringTranslator("bar") },
                    { (source) => true, new StringTranslator("baz") }
                });

            var result = translator.Translate("foo");

            Assert.Equal("baz", result);
        }

        private sealed class StringTranslator : ITranslatable<string, string>
        {
            private readonly string _result;

            public StringTranslator(string result)
            {
                _result = result;
            }

            public string Translate(string args)
            {
                return _result;
            }
        }
    }
}
