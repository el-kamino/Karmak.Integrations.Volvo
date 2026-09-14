using System.Xml.Linq;

namespace Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml
{
    public static class XElementExtended
    {
        public static XElement[] ParseEach(params string[] elementStrings) =>
            elementStrings.Select(XElement.Parse).ToArray();
    }
}
