using System;
using System.Xml;

namespace Karmak.Integrations.Volvo.React.Transport.Utils
{
    public static class XmlHelpers {
        public static bool HasElement(XmlDocument document, string elementLocalName, string xmlNamespaceUrl) {
            return document.GetElementsByTagName(elementLocalName, xmlNamespaceUrl).Count >= 1;
        }

        public static XmlElement GetElement(XmlDocument document, string elementLocalName, string xmlNamespaceUrl) {
            var elements = document.GetElementsByTagName(elementLocalName, xmlNamespaceUrl);
            if (elements.Count < 1) {
                throw new ArgumentException($"No {elementLocalName} elements found.");
            }
            return (XmlElement)elements[0];
        }
    }
}