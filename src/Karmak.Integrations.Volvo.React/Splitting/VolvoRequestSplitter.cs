using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Splitting
{
    public class VolvoRequestSplitter : IVolvoRequestSplitter
    {
        private readonly ISplittingCriteria<XDocument> _splittingCriteria;
        private readonly ISplittingStrategy<XDocument> _splittingStrategy;
        private readonly string _bodIdXPath;
        private readonly string _messageIdXPath;

        public VolvoRequestSplitter(
            ISplittingCriteria<XDocument> splittingCriteria,
            ISplittingStrategy<XDocument> splittingStrategy)
        {
            _splittingCriteria = splittingCriteria;
            _splittingStrategy = splittingStrategy;
            _bodIdXPath = $"//{XmlNamespaces.Star.Prefix}:BODID";
            _messageIdXPath = $"//{XmlNamespaces.WsAddressing.Prefix}:MessageID";
        }

        public IEnumerable<SoapEnvelope> Split(SoapEnvelope envelope)
        {
            var xdoc = envelope.ToXDocument();
            return SplitRecursively(xdoc)
                .Select(SetUniqueIdsPerDocument)
                .Select(doc => doc.FromXDocument<SoapEnvelope>()).ToArray();
        }

        private IEnumerable<XDocument> SplitRecursively(XDocument message)
        {
            if (_splittingCriteria.IsComplete(message))
            {
                return new[] {
                    message
                };
            }
            var ret = new List<XDocument>();
            var (left, right) = _splittingStrategy.Split(message);
            ret.AddRange(SplitRecursively(left));
            ret.AddRange(SplitRecursively(right));
            return ret;
        }

        private XDocument SetUniqueIdsPerDocument(XDocument doc)
        {
            var bodId = doc.XPathSelectElement(_bodIdXPath, XmlNamespaces.GetManager());
            bodId.SetValue(Guid.NewGuid());
            var messageId = doc.XPathSelectElement(_messageIdXPath, XmlNamespaces.GetManager());
            messageId.SetValue(Guid.NewGuid());
            return doc;
        }
    }
}