using Karmak.Integrations.Volvo.React.Transport.Soap;
using Xunit;

namespace Elk.Integrations.Volvo.Communications.Transport.Tests.Soap {
    public class ToXmlTest {
        [Fact]
        public void Serializes_element_escapes_special_characters()
        {
            var result = ToXml.Element(new TestClass{ MyKey = "<Test>&!@XML" });
            var str = result.InnerXml;

            Assert.Equal("<MyKey>&lt;Test&gt;&amp;!@XML</MyKey>", str);
        }

        public class TestClass {
            public string MyKey { get; set; }
        }
    }
}
