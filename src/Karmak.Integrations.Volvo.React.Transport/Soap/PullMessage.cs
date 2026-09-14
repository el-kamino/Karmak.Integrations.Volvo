using System.Xml;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.TransportUrl)]
    public class PullMessage {
        [XmlAnyElement]
        public XmlElement Value { get; set; }
    }
}