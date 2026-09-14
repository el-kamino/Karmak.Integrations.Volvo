using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.TransportUrl)]
    public class ProcessMessage {
        [XmlElement(ElementName = "payload")]
        public Payload Payload { get; set; }
    }
}