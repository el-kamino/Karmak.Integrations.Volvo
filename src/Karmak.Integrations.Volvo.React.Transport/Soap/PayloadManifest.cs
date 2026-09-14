using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.TransportUrl)]
    public class PayloadManifest {
        [XmlElement(ElementName = "manifest")]
        public Manifest ManifestElement { get; set; } = new Manifest();

        [XmlIgnore]
        public string Id {
            get => ManifestElement.Id;
        }

        [XmlIgnore]
        public string Element {
            get => ManifestElement.Element;
            set => ManifestElement.Element = value;
        }

        [XmlIgnore]
        public string NamespaceUri {
            get => ManifestElement.NamespaceUri;
            set => ManifestElement.NamespaceUri = value;
        }

        [XmlIgnore]
        public string Version {
            get => ManifestElement.Version;
            set => ManifestElement.Version = value;
        }

        [XmlRoot(Namespace = XmlNamespaces.TransportUrl)]
        public class Manifest {
            [XmlAttribute(AttributeName = "contentID")]
            public string Id { get; set; } = Content.ID;

            [XmlAttribute(AttributeName = "element")]
            public string Element { get; set; }

            [XmlAttribute(AttributeName = "namespaceURI")]
            public string NamespaceUri { get; set; }

            [XmlAttribute(AttributeName = "version")]
            public string Version { get; set; }
        }
    }

}