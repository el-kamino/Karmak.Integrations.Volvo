using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class StarXmlExtensions
    {
        public const string STAR_NAMESPACE = "http://www.starstandard.org/STAR/5";
        public const string OAGIS_NAMESPACE = "http://www.openapplications.org/oagis/9";
        private static readonly XmlSerializerNamespaces XmlSerializerNamespaces = new XmlSerializerNamespaces(new[] {
            new XmlQualifiedName("star", STAR_NAMESPACE),
            new XmlQualifiedName("oagis", OAGIS_NAMESPACE)
        });

        public static XDocument ToXDocument<T>(this T obj)
        {
            var t = typeof(T);
            var document = new XDocument();
            using (var writer = document.CreateWriter())
            {
                var serializer = new XmlSerializer(t);
                serializer.Serialize(writer, obj, XmlSerializerNamespaces);
            }
            return document;
        }

        public static T FromXDocument<T>(this XDocument doc) {
            var serializer = new XmlSerializer(typeof(T));
            return (T) serializer.Deserialize(doc.CreateReader());
        }
    }
}