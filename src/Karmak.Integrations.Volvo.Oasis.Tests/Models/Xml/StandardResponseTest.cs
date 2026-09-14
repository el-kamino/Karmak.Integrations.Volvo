using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Oasis.Tests.Models.Xml
{
    public class StandardResponseTest
    {
        [Fact]
        public void CanDeserializeAnOasisValidVehicleResponseDirectlyToJson()
        {
            var result = SerializationUtils.DeserializeXmlToJson(OasisSampleResponses.ValidVehicleResponse);
            Assert.NotNull(result);
            var json = JObject.Parse(result);
            Assert.NotNull(json);
        }

        [Fact]
        public void CanDeserializeAnOasisCompleteResponseDirectlyToJson()
        {
            var result = SerializationUtils.DeserializeXmlToJson(OasisSampleResponses.CompleteVehicleResponse);
            Assert.NotNull(result);
            var json = JObject.Parse(result);
            Assert.NotNull(json);
        }

        [Fact]
        public void CanDeserializeAnOasisResponseWithUnknownDirectlyToJson()
        {
            var result = SerializationUtils.DeserializeXmlToJson(OasisSampleResponses.ResponseWithUnknownXml);
            Assert.NotNull(result);
            var json = JObject.Parse(result);
            Assert.NotNull(json);
        }
    }
}
