using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Karmak.Integrations.Volvo.React.Transport.Soap;

namespace Karmak.Integrations.Volvo.React.Splitting
{
    public class HasNElementsAtXPath : ISplittingCriteria<XDocument> {
        private readonly XmlNamespaceManager _xmlNamespaceManager = XmlNamespaces.GetManager();
        private readonly string _xpath;
        private readonly int _n;

        public HasNElementsAtXPath(string xpath, int n) {
            _xpath = xpath;
            _n = n;
        }

        public bool IsComplete(XDocument message) {
            return message.XPathSelectElements(_xpath, _xmlNamespaceManager).Count() == _n;
        }
    }
}