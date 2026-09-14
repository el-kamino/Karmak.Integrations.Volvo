using System.Xml;

namespace Karmak.Integrations.Volvo.Warranty.Tests.TestUtils
{
    public static class XmlHelpers
    {
        public static bool HasElement(XmlDocument document, string localName, string namespaceUri)
        {
            return document.GetElementsByTagName(localName, namespaceUri).Count > 0;
        }
    }
}
