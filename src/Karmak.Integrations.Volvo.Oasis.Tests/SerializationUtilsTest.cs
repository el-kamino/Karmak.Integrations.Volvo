using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Oasis.Tests
{
    public class SerializationUtilsTest
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void DeserializeXmlToJsonHandlesNullOrEmptyXml(string xml)
        {
            var result = SerializationUtils.DeserializeXmlToJson(xml);
            Assert.Null(result);
        }

        [Fact]
        public void DeserializeXmlToJsonRemovesXmlVersionInfo()
        {
            const string sampleXml = @"<?xml version='1.0' encoding='UTF-8'?>
                <GOASIS xmlns='http://volvo.com/goasis'>
            <GOASIS-RESULT-LIT>OASIS RESULT:</GOASIS-RESULT-LIT>
            <DLR-LANG-RESP>
            <RET-LANG-CODE>EN</RET-LANG-CODE>
            <RET-LANG-DESC>ENGLISH</RET-LANG-DESC>
            <LANG-RESP-MSG />
            </DLR-LANG-RESP>
            </GOASIS>";

            var result = SerializationUtils.DeserializeXmlToJson(sampleXml);
            Assert.DoesNotContain("version", result);
            Assert.DoesNotContain("encoding", result);
        }

        [Fact]
        public void DeserializeXmlToJsonRemovesNamespaces()
        {
            const string sampleXml = @"<?xml version='1.0' encoding='UTF-8'?>
                <GOASIS xmlns='http://volvo.com/goasis'>
            <GOASIS-RESULT-LIT>OASIS RESULT:</GOASIS-RESULT-LIT>
            <DLR-LANG-RESP>
            <RET-LANG-CODE>EN</RET-LANG-CODE>
            <RET-LANG-DESC>ENGLISH</RET-LANG-DESC>
            <LANG-RESP-MSG />
            </DLR-LANG-RESP>
            </GOASIS>";

            var result = SerializationUtils.DeserializeXmlToJson(sampleXml);
            Assert.DoesNotContain("http://volvo.com/goasis", result);
        }

        [Fact]
        public void DeserializeXmlToJsonConvertsXmlTagsToJsonFieldsAsExpected()
        {
            const string sampleXml = @"<?xml version='1.0' encoding='UTF-8'?>
                <GOASIS xmlns='http://volvo.com/goasis'>
            <GOASIS-RESULT-LIT>OASIS RESULT:</GOASIS-RESULT-LIT>
            <DLR-LANG-RESP>
            <ID>1234</ID>
            <RET-LANG-CODE IS_PRIMARY='true'>EN</RET-LANG-CODE>
            <RET-LANG-DESC>ENGLISH</RET-LANG-DESC>
            <LANG_RESP_MSG />
            </DLR-LANG-RESP>
            </GOASIS>";

            var result = SerializationUtils.DeserializeXmlToJson(sampleXml);
            Assert.Contains("goasis", result);
            Assert.Contains("goasisResultLit", result);
            Assert.Contains("dlrLangResp", result);
            Assert.Contains("id", result);
            Assert.Contains("retLangCode", result);
            Assert.Contains("isPrimary", result);
            Assert.Contains("retLangDesc", result);
            Assert.Contains("langRespMsg", result);

            var json = JObject.Parse(result);
            Assert.Equal("OASIS RESULT:", json.SelectToken("goasis.goasisResultLit"));
            Assert.Equal("1234", json.SelectToken("goasis.dlrLangResp.id"));
            Assert.Equal("true", json.SelectToken("goasis.dlrLangResp.retLangCode.isPrimary"));
            Assert.Equal("EN", json.SelectToken("goasis.dlrLangResp.retLangCode.value"));
            Assert.Equal("ENGLISH", json.SelectToken("goasis.dlrLangResp.retLangDesc"));
            Assert.Equal("", json.SelectToken("goasis.dlrLangResp.langRespMsg"));
        }
    }
}
