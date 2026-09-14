using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Splitting
{
    public class SplitXDocumentOnXPath : ISplittingStrategy<XDocument> {
        private readonly XmlNamespaceManager _xmlNamespaceManager = XmlNamespaces.GetManager();
        private readonly string _xpath;

        public SplitXDocumentOnXPath(string xpath) {
            _xpath = xpath;
        }

        public (XDocument, XDocument) Split(XDocument message) {
            var elements = message.XPathSelectElements(_xpath, _xmlNamespaceManager).ToArray();
            var (leftElements, rightElements) = elements.SplitEvenly();
            var left = CreateDuplicateMessageWithElements(message, leftElements);
            var right = CreateDuplicateMessageWithElements(message, rightElements);
            return (left, right);
        }

        private XDocument CreateDuplicateMessageWithElements(XDocument template, IEnumerable<XElement> newElements) {
            var document = new XDocument(template);
            var parent = document.XPathSelectElement($"{_xpath}/parent::*", _xmlNamespaceManager);
            var newChildren = parent.Elements()
                .Where(el => !el.Name.Equals(document.XPathSelectElement(_xpath, _xmlNamespaceManager).Name))
                .Concat(newElements);
            parent.ReplaceNodes(newChildren);
            return document;
        }
    }
}