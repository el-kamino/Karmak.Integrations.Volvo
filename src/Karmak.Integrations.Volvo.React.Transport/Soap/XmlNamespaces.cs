using System.Xml;
using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    public static class XmlNamespaces {
        public const string SoapUrl = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string VolvoIdUrl = "urn:volvo/star/security/v1.0";
        public const string VolvoRoutingUrl = "urn:volvo/soa/routing/v1.0";
        public const string WsAddressingUrl = "http://www.w3.org/2005/08/addressing";
        public const string WsSecurityUtilityUrl = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        public const string WsSecurityUrl = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public const string TransportUrl = "http://www.starstandards.org/webservices/2005/10/transport";
        public const string SignatureUrl = "http://www.w3.org/2000/09/xmldsig#";
        public const string EncryptionUrl = "http://www.w3.org/2001/04/xmlenc#";
        public const string StarUrl = "http://www.starstandard.org/STAR/5";
        public const string OagisUrl = "http://www.openapplications.org/oagis/9";

        public static readonly XmlNamespace Oagis = new XmlNamespace("ns1", OagisUrl);
        public static readonly XmlNamespace Star = new XmlNamespace("ns", StarUrl);
        public static readonly XmlNamespace Soap = new XmlNamespace("soap", SoapUrl);
        public static readonly XmlNamespace WsAddressing = new XmlNamespace("wsa", WsAddressingUrl);
        public static readonly XmlNamespace WsSecurityUtility = new XmlNamespace("wsu", WsSecurityUtilityUrl);
        public static readonly XmlNamespace WsSecurity = new XmlNamespace("wsse", WsSecurityUrl);
        public static readonly XmlNamespace Encryption = new XmlNamespace("xenc", EncryptionUrl);
        public static readonly XmlNamespace Signature = new XmlNamespace("ds", SignatureUrl);
        public static readonly XmlNamespace Transport = new XmlNamespace("tran", TransportUrl);
        public static readonly XmlNamespace VolvoRouting = new XmlNamespace("tns", VolvoRoutingUrl);
        public static readonly XmlNamespace RespondTo = new XmlNamespace("volvoID", VolvoIdUrl);

        public static XmlSerializerNamespaces GetNamespaces() {
            var namespaces = new XmlSerializerNamespaces();
            foreach (var field in typeof(XmlNamespaces).GetFields()) {
                if (!(field.GetValue(null) is XmlNamespace xmlNamespace)) {
                    continue;
                }
                namespaces.Add(xmlNamespace.Prefix, xmlNamespace.Url);
            }
            return namespaces;
        }

        public static XmlNamespaceManager GetManager() {
            var manager = new XmlNamespaceManager(new NameTable());
            foreach (var field in typeof(XmlNamespaces).GetFields()) {
                if (!(field.GetValue(null) is XmlNamespace xmlNamespace)) {
                    continue;
                }
                manager.AddNamespace(xmlNamespace.Prefix, xmlNamespace.Url);
            }
            return manager;
        }

        public class XmlNamespace {
            public string Prefix { get; }
            public string Url { get; }

            public XmlNamespace(string prefix, string url) {
                Prefix = prefix;
                Url = url;
            }
        }
    }
}