using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.VolvoIdUrl)]
    public class VolvoDealerIdentity {
        [XmlElement(ElementName = "SiteCode", Namespace = XmlNamespaces.VolvoIdUrl)]
        public Code SiteCodeElement { get; set; } = new Code();

        [XmlIgnore]
        public string SiteCode {
            get => SiteCodeElement.Value;
            set => SiteCodeElement.Value = value;
        }

        [XmlRoot(Namespace = XmlNamespaces.VolvoIdUrl)]
        public class Code {
            [XmlText] public string Value { get; set; }
        }
    }
}