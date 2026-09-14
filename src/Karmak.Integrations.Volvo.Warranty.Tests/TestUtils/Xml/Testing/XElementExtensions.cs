using System.Xml.Linq;
using Xunit.Sdk;

namespace Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing
{
    public static class XElementExtensions
    {
        public static XElement SortAttributes(this XElement element) =>
            new XElement(element.Name, element.Attributes().OrderBy(a => a.Name.ToString()));

        public static void AssertContains(this IEnumerable<XElement> sourceElements, XElement element)
        {
            var elements = sourceElements as XElement[] ?? sourceElements.ToArray();
            if (!Contains(elements, element))
            {
                throw new XunitException($"Assert.Contains() Failure\nExpected: {element}\nActual: {ToXmlString(elements)}");
            }
        }

        public static void AssertNotContains(this IEnumerable<XElement> sourceElements, XElement element)
        {
            var elements = sourceElements as XElement[] ?? sourceElements.ToArray();
            if (Contains(elements, element))
            {
                throw new XunitException($"Assert.DoesNotContain() Failure\nNot expected: {element}\nActual: {ToXmlString(elements)}");
            }
        }

        public static void AssertContainsEach(this IEnumerable<XElement> sourceElements, IEnumerable<XElement> elements)
        {
            if (!ContainsEach(sourceElements, elements))
            {
                throw new XunitException($"Assert.Contains() Failure\nExpected each of: {string.Join(", ", elements.Select(e => e.ToString()))}\nActual: {string.Join(", ", sourceElements.Select(e => e.ToString()))}");
            }
        }

        public static bool Contains(this IEnumerable<XElement> sourceElements, XElement element) =>
            sourceElements
                .Descendants(element.Name)
                .Any(match => Equals(match.ToString(), element.ToString()));

        public static bool ContainsEach(this IEnumerable<XElement> sourceElements, IEnumerable<XElement> elements) =>
            elements.All(element => Contains(sourceElements, element));

        private static string ToXmlString(IEnumerable<XElement> sourceElements) =>
            string.Join(Environment.NewLine, sourceElements);
    }
}
