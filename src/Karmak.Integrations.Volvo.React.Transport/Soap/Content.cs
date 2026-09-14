using System.Xml;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    public class Content {
        public const string ID = "Content";

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; } = ID;

        [XmlAnyElement]
        public XmlElement Value { get; set; }
    }
}