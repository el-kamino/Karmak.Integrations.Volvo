using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.SoapUrl)]
    public class Header {
        [XmlElement(Namespace = XmlNamespaces.WsSecurityUrl)]
        public Security Security { get; set; }

        [XmlElement(ElementName = "payloadManifest", Namespace = XmlNamespaces.TransportUrl)]
        public PayloadManifest PayloadManifest { get; set; }

        [XmlElement(Namespace = XmlNamespaces.WsAddressingUrl)]
        public string To { get; set; }

        [XmlElement(Namespace = XmlNamespaces.WsAddressingUrl)]
        public string Action { get; set; }

        [XmlElement(Namespace = XmlNamespaces.WsAddressingUrl)]
        public string MessageID { get; set; }

        [XmlElement(Namespace = XmlNamespaces.VolvoIdUrl)]
        public VolvoDealerIdentity VolvoDealerIdentity { get; set; }

        [XmlElement(Namespace = XmlNamespaces.VolvoRoutingUrl)]
        public RespondTo RespondTo { get; set; }
    }
}