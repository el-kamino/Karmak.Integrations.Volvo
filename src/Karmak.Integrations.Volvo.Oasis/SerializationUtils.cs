using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Oasis
{
    public static class SerializationUtils
    {
        private const string DEFAULT_XML_NS = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"";
        private const string NIL_TAG = " xsi:nil=\"true\"";

        public static string Serialize<T>(T objectToSerialize)
        {
            var xmlSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            };

            // If you land here because of an exception about being unable to load an assembly,
            // just ignore it. See for more: https://stackoverflow.com/a/1177040/11262494
            var serializer = new XmlSerializer(typeof(T));
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            using (var xmlWriter = XmlWriter.Create(writer, xmlSettings))
            {
                serializer.Serialize(xmlWriter, objectToSerialize);
            }

            sb.Replace(DEFAULT_XML_NS, string.Empty);
            sb.Replace(NIL_TAG, string.Empty);
            return sb.ToString();
        }

        public static T Deserialize<T>(string xmlToDeserialize)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            using (TextReader reader = new StringReader(xmlToDeserialize))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        public static string DeserializeXmlToJson(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;
            var doc = XDocument.Parse(xml);
            var root = RemoveNamespaces(doc.Root);

            var json = JsonConvert
                .SerializeXNode(root)
                .Replace("@", "")
                .Replace("#text", "value");
            return json;
        }

        private static XElement RemoveNamespaces(XElement element)
        {
            if (!element.HasElements)
            {
                var cleanElement = new XElement(BuildFieldName(element.Name))
                {
                    Value = element.Value
                };

                var attributes = element.Attributes().Where(a => !a.IsNamespaceDeclaration);
                if (attributes.Any())
                    cleanElement.Add(attributes.Select(attr => new XAttribute(BuildFieldName(attr.Name), attr.Value)));
                return cleanElement;
            }

            return new XElement(BuildFieldName(element.Name), element.Elements().Select(RemoveNamespaces));
        }

        private static string BuildFieldName(XName nodeName)
        {
            if (nodeName == null)
                return "missingFieldName";

            var name = nodeName.LocalName;

            if (!name.Contains("-") && !name.Contains("_"))
                return name.ToLower();

            var nameParts = name.Contains("-") ? name.Split("-") : name.Split("_");
            if (nameParts.Length == 1)
                return nameParts[0].ToLower();
            return nameParts
                .Skip(1)
                .Aggregate(
                    new StringBuilder(nameParts[0].ToLower()),
                    (acc, next) => acc.Append(CapitalizeFirstLetter(next))
                )
                .ToString();
        }

        // this first-letter capitalization approach came from https://stackoverflow.com/a/27073919/11262494
        private static char[] CapitalizeFirstLetter(string value)
        {
            var lower = value.ToLower().ToCharArray();
            lower[0] = char.ToUpper(lower[0]);
            return lower;
        }
    }
}
