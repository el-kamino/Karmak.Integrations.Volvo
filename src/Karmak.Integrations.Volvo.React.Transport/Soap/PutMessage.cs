using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.TransportUrl)]
    public class PutMessage {
        [XmlElement(ElementName = "payload")]
        public Payload Payload { get; set; }
    }
}