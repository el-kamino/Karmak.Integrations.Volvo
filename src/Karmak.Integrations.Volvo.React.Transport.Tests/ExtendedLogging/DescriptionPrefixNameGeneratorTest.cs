using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using System.Text;

namespace Elk.Integrations.Volvo.Communications.Transport.Tests.ExtendedLogging {
    public class DescriptionPrefixNameGeneratorTest {
        [Fact]
        public void WhenDescriptionProvidedItGeneratesName() {
            var stubRequest = new ExtendedLoggingRequest() {
                Application = "stubApplication",
                Content = Encoding.UTF8.GetBytes("stubContent"),
                ContentType = "stubContentType",
                Module = "stubModule",
                Metadata = new Dictionary<string, string> {
                    {"ContentDescription", "Stub Content Description" }
                }
            };
            var generator = new DescriptionPrefixNameGenerator(new StubNameGenerator());

            var result = generator.Generate(stubRequest);

            Assert.Equal("stub-content-description-stub-guid", result);
        }
        [Fact]
        public void WhenDescriptionIsNotProvidedItUsesTheGenerator() {
            var stubRequest = new ExtendedLoggingRequest() {
                Application = "stubApplication",
                Content = Encoding.UTF8.GetBytes("stubContent"),
                ContentType = "stubContentType",
                Module = "stubModule",
                Metadata = new Dictionary<string, string>()
            };
            var generator = new DescriptionPrefixNameGenerator(new StubNameGenerator());

            var result = generator.Generate(stubRequest);

            Assert.Equal("stub-guid", result);
        }


        private class StubNameGenerator : IExtendedLogNameGenerator {
            public string Generate(ExtendedLoggingRequest request) {
                return "stub-guid";
            }
        }
    }
}
