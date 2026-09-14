using System.Xml.Serialization;
using Karmak.Integrations.Volvo.React.Transport.Soap;

namespace Karmak.Integrations.Volvo.Warranty.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.SoapUrl)]
    public class Fault
    {
        [XmlElement(ElementName = "faultcode", Namespace = XmlNamespaces.SoapUrl)]
        public string FaultCode { get; set; }

        [XmlElement(ElementName = "faultstring", Namespace = XmlNamespaces.SoapUrl)]
        public string FaultString { get; set; }

        [XmlElement(ElementName = "detail", Namespace = XmlNamespaces.SoapUrl)]
        public string Detail { get; set; } = "";
    }
}
