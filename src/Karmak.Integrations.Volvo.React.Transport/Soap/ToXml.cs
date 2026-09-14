using System.Xml;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    public static class ToXml {

        public static XmlDocument Document<T>(T input) {
            var doc = new XmlDocument {
                PreserveWhitespace = true
            };
            var nav = doc.CreateNavigator();

            using (var writer = nav.AppendChild()) {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, input, XmlNamespaces.GetNamespaces());
            }

            return doc;
        }

        public static XmlElement Element<T>(T input) {
            return Document(input).DocumentElement;
        }

    }
}