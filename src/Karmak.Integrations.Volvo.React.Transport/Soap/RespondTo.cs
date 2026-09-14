using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.VolvoRoutingUrl)]
    public class RespondTo {
        [XmlElement(Namespace = XmlNamespaces.VolvoRoutingUrl)]
        public string Endpoint { get; set; }
    }
}
