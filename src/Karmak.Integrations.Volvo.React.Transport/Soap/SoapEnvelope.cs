using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.SoapUrl, ElementName = "Envelope")]
    public class SoapEnvelope {
        [XmlElement]
        public Header Header { get; set; }

        [XmlElement]
        public Body Body { get; set; }
    }
}