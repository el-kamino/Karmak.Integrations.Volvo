using System.Xml.Linq;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml
{
    public static class XDocumentExtended
    {
        public static XDocument Serialize<T>(T source)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, source);
                return XDocument.Parse(writer.ToString());
            }
        }
    }
}
