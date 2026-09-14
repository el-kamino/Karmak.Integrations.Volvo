using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.WsSecurityUrl)]
    public class BinarySecurityToken {
        [XmlAttribute(Namespace = XmlNamespaces.WsSecurityUtilityUrl)]
        public string Id { get; set; }

        [XmlAttribute]
        public string EncodingType { get; set; }

        [XmlAttribute]
        public string ValueType { get; set; }

        [XmlText]
        public string Value { get; set; }

        public static BinarySecurityToken For(X509Certificate2 cert) {
            return new BinarySecurityToken {
                Id = Guid.NewGuid().ToString(),
                EncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary",
                ValueType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3",
                Value = Convert.ToBase64String(cert.Export(X509ContentType.Cert))
            };
        }
    }
}