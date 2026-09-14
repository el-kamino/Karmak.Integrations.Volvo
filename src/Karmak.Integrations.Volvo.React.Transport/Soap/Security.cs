using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.WsSecurityUrl)]
    public class Security {
        [XmlAttribute(Namespace = XmlNamespaces.SoapUrl, AttributeName = "mustUnderstand")]
        public const string MustUnderstand = "1";

        [XmlElement(ElementName = "BinarySecurityToken")]
        public List<BinarySecurityToken> CertTokens { get; } = new List<BinarySecurityToken>();

        [XmlAnyElement(Name = "EncryptedKey", Namespace = XmlNamespaces.EncryptionUrl)]
        public XmlElement EncryptedKey { get; set; }

        [XmlAnyElement(Name = "Signature", Namespace = XmlNamespaces.SignatureUrl)]
        public XmlElement Signature { get; set; }

        [XmlElement(Namespace = XmlNamespaces.WsSecurityUtilityUrl)]
        public Timestamp Timestamp { get; set; }
    }
}