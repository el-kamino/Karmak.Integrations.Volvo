using System;
using System.Xml;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.SoapUrl)]
    public class Body {
        [XmlAttribute(Namespace = XmlNamespaces.WsSecurityUtilityUrl)]
        public string Id { get; set; }

        [XmlAnyElement(Namespace = XmlNamespaces.TransportUrl)]
        public XmlElement Value { get; set; }

        public Body() {
            Id = Guid.NewGuid().ToString();
        }
    }
}