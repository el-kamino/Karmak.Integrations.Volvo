using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.TransportUrl)]
    public class Payload {
        [XmlElement(ElementName = "content")]
        public Content Content { get; set; }
    }
}