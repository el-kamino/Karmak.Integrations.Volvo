using System.Xml.Linq;

namespace Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml
{
    public static class XNameExtended
    {
        public static string ToXmlString(this XName elementName) =>
            $"<{elementName.LocalName} xmlns=\"{elementName.Namespace}\" />";
    }
}
