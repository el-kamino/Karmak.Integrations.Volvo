using System.Xml.Linq;
using Xunit.Sdk;

namespace Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing
{
    public static class XDocumentExtensions
    {
        public static void AssertContains(this XDocument doc, XElement element)
        {
            if (!Contains(doc, element))
            {
                throw new XunitException($"Assert.Contains() Failure\nExpected: {element}\nActual: {doc}");
            }
        }

        public static void AssertContainsName(this XDocument doc, XName elementName)
        {
            if (!doc.Descendants(elementName).Any())
            {
                throw new XunitException($"Assert.Contains() Failure\nExpected: {elementName.ToXmlString()}\nActual: {doc}");
            }
        }

        public static void AssertContainsEmpty(this XDocument doc, XName elementName)
        {
            if (!doc.Descendants(elementName).Any(e => e.IsEmpty))
            {
                throw new XunitException($"Assert.Contains() Failure\nExpected: {elementName.ToXmlString()}\nActual: {doc}");
            }
        }

        public static void AssertContainsIn(this XDocument doc, XName parentElementName, XElement element) =>
            doc.Descendants(parentElementName).AssertContains(element);

        public static void AssertNotContainsIn(this XDocument doc, XName parentElementName, XElement element) =>
            doc.Descendants(parentElementName).AssertNotContains(element);

        public static void AssertContainsEach(this XDocument doc, params XElement[] elements)
        {
            if (!ContainsEach(doc, elements))
            {
                throw new XunitException($"Assert.Contains() Failure\nExpected each of: {string.Join(", ", elements.Select(e => e.ToString()))}\nActual: {doc}");
            }
        }

        public static void AssertContainsEachIn(this XDocument doc, XName elementName, params XElement[] elements) =>
            doc.Descendants(elementName).AssertContainsEach(elements);

        public static void AssertContainsName(this XDocument doc, XName parentElementName, XName elementName)
        {
            if (!doc.ContainsName(parentElementName, elementName))
            {
                throw new XunitException($"Assert.Contains() Failure\nExpected: {elementName.ToXmlString()}\nActual: {doc.DescendentXmlString(parentElementName)}");
            }
        }

        public static void AssertNotContainsName(this XDocument doc, XName parentElementName, XName elementName)
        {
            if (doc.ContainsName(parentElementName, elementName))
            {
                throw new XunitException($"Assert.DoesNotContain() Failure\nNot expected: {elementName.ToXmlString()}\nActual: {doc.DescendentXmlString(parentElementName)}");
            }
        }

        public static void AssertContainsWithAttributes(this XDocument doc, XElement element)
        {
            if (!ContainsWithAttributes(doc, element))
            {
                throw new XunitException($"Assert.Contains() Failure\nExpected: {element}\nActual: {string.Join(Environment.NewLine, doc.Descendants(element.Name))}");
            }
        }

        public static bool ContainsName(this XDocument doc, XName parentElementName, XName elementName) =>
            doc
                .Descendants(parentElementName)
                .Descendants(elementName)
                .Any();

        public static bool Contains(this XDocument doc, XElement element) =>
            doc.Descendants(element.Name)
                .Any(match => Equals(match.ToString(), element.ToString()));

        public static bool ContainsEach(this XDocument doc, params XElement[] elements) =>
            elements.All(element => Contains(doc, element));

        public static bool ContainsWithAttributes(this XDocument doc, XElement element) =>
            doc.Descendants(element.Name)
                .Any(match => Equals(match.SortAttributes().ToString(), element.SortAttributes().ToString()));

        private static string DescendentXmlString(this XContainer doc, XName parentElementName) =>
            string.Join(Environment.NewLine, doc.Descendants(parentElementName));
    }
}
