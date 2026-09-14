using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Storage.Table;
using NSubstitute;
using System.Collections.Generic;

namespace Integrations.Inbox.Core.Test
{
    public class MockMetadataClient
    {
        private readonly List<MetaMessageEnvelope> _messages;
        private readonly IMetaDataStore<MetaMessageEnvelope> _metadataClient;

        public MockMetadataClient(List<MetaMessageEnvelope> messages)
        {
            _messages = messages;
            _metadataClient = Substitute.For<IMetaDataStore<MetaMessageEnvelope>>();
        }

        private List<MetaMessageEnvelope> BuildSinglePageResults()
        {
            return new List<MetaMessageEnvelope>(_messages);
        }

        public IMetaDataStore<MetaMessageEnvelope> BuildSinglePageResultsTableClient()
        {
            _metadataClient.QueryAsync(Arg.Any<string>())
                .ReturnsForAnyArgs(BuildSinglePageResults());

            return _metadataClient;
        }
    }
}