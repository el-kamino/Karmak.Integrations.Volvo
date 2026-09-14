using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Inbox;
using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Karmak.Integrations.Volvo.Inbox.Validators.Blob;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Integrations.Inbox.Core.Test
{
    public class InboxServiceQueryTest
    {
        public InboxServiceQueryTest()
        {
            _elkContext = new ElkContext.Builder
            {
                Identity = new ElkIdentity.Builder
                {
                    Account = ACCOUNT_ID,
                    User = USER_ID
                }.Build(),
                ApplicationContext = new ElkApplicationContext.Builder
                {
                    Branch = BRANCH_ID,
                    Instance = INSTANCE_ID
                }.Build(),
                SecurityProfile = new ElkIdentity.Builder
                {
                    Account = ACCOUNT_ID,
                    User = USER_ID
                }.Build()
            }.Build();
        }

        private static readonly Guid ACCOUNT_ID = Guid.Parse("A0D9E6B9-C2E6-4035-A6BC-260CBE54E4B5");
        private static readonly Guid INSTANCE_ID = Guid.Parse("1E6841C0-25D3-488C-A2DF-2F0920149C39");
        private static readonly Guid BRANCH_ID = Guid.Parse("A3F79084-DE28-431E-AF74-578F07FA0E1C");
        private static readonly Guid USER_ID = Guid.Parse("F6A1C40B-3606-49FB-8931-7864609394B9");

        private const int ITEMS_PER_PAGE = 2;
        private readonly ElkContext _elkContext;

        [Fact]
        public async Task CallsTableStorageWithFormattedArguments()
        {
            var metadataStore = new MockMetadataClient(new List<MetaMessageEnvelope>()).BuildSinglePageResultsTableClient();
            var filter = "";
            metadataStore.QueryAsync(Arg.Do<string>(y => filter = y));

            var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 10, 9, 14, 25, 59, TimeSpan.Zero));

            var service = new InboxService(
                new InboxRepository(metadataStore, timeProvider, Options.Create(new InboxRepositoryOptions { EarliestNumberOfDaysToRetrieveMessages = 30 })),
                Substitute.For<IStorageClient>(),
                NullLogger<InboxService>.Instance);

            await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await service.GetAllInboxMessagesAsync(new PagingArguments {PageSize = ITEMS_PER_PAGE}));

            var earliestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().Date.AddDays(-30).Ticks;
            var latestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().AddDays(1).Ticks;
            Assert.Contains(INSTANCE_ID.ToString(), filter);
            Assert.Contains(BRANCH_ID.ToString(), filter);
            Assert.Contains(earliestDateTicks.ToString(), filter);
            Assert.Contains(latestDateTicks.ToString(), filter);
        }

        [Fact]
        public async Task CallsTableStorageWithNoActiveOnlyFilterByDefault()
        {
            var metadataStore = new MockMetadataClient(new List<MetaMessageEnvelope>()).BuildSinglePageResultsTableClient();
            var filter = "";
            metadataStore.QueryAsync(Arg.Do<string>(y => filter = y));

            var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 10, 9, 14, 25, 59, TimeSpan.Zero));

            var service = new InboxService(
                new InboxRepository(metadataStore, timeProvider, Options.Create(new InboxRepositoryOptions { EarliestNumberOfDaysToRetrieveMessages = 30 })),
                Substitute.For<IStorageClient>(),
                NullLogger<InboxService>.Instance);

            await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await service.GetAllInboxMessagesAsync(new PagingArguments
                {
                    PageSize = ITEMS_PER_PAGE
                }));

            var earliestDateTicks = DateTime.MaxValue.Ticks -
                                    timeProvider.GetUtcNow().Date
                                        .AddDays(30)
                                        .Ticks;
            var latestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().AddDays(1).Ticks;
            Assert.DoesNotContain("IsActive eq true", filter);
        }

        [Fact]
        public async Task CallsTableStorageWithActiveOnlyFilter()
        {
            var metadataStore = new MockMetadataClient(new List<MetaMessageEnvelope>()).BuildSinglePageResultsTableClient();
            var filter = "";
            metadataStore.QueryAsync(Arg.Do<string>(y => filter = y));

            var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 10, 9, 14, 25, 59, TimeSpan.Zero));

            var service = new InboxService(
                new InboxRepository(metadataStore, timeProvider, Options.Create(new InboxRepositoryOptions { EarliestNumberOfDaysToRetrieveMessages = 30 })),
                Substitute.For<IStorageClient>(),
                NullLogger<InboxService>.Instance);

            await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await service.GetAllInboxMessagesAsync(new PagingArguments
                {
                    PageSize = ITEMS_PER_PAGE,
                    ActiveOnly = true
                }));

            var earliestDateTicks = DateTime.MaxValue.Ticks -
                                      timeProvider.GetUtcNow().Date
                                          .AddDays(30)
                                          .Ticks;
            var latestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().AddDays(1).Ticks;
            Assert.Contains("IsActive eq true", filter);
        }

        [Fact]
        public async Task CallsTableStorageWithoutIsActiveFilterIfActiveOnlyIsFalse()
        {
            var metadataStore = new MockMetadataClient(new List<MetaMessageEnvelope>()).BuildSinglePageResultsTableClient();
            var filter = "";
            metadataStore.QueryAsync(Arg.Do<string>(y => filter = y));

            var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 10, 9, 14, 25, 59, TimeSpan.Zero));

            var service = new InboxService(
                new InboxRepository(metadataStore, timeProvider, Options.Create(new InboxRepositoryOptions { EarliestNumberOfDaysToRetrieveMessages = 30 })),
                Substitute.For<IStorageClient>(),
                NullLogger<InboxService>.Instance);

            await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await service.GetAllInboxMessagesAsync(new PagingArguments
                {
                    PageSize = ITEMS_PER_PAGE,
                    ActiveOnly = false
                }));

            var earliestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().Date .AddDays(30).Ticks;
            var latestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().AddDays(1).Ticks;
            Assert.DoesNotContain("IsActive", filter);
        }

        [Fact]
        public async Task CallsTableStorageWithoutIsActiveFilterIfActiveOnlyIsNotExplicitlySet()
        {
            var metadataStore = new MockMetadataClient(new List<MetaMessageEnvelope>()).BuildSinglePageResultsTableClient();
            var filter = "";
            metadataStore.QueryAsync(Arg.Do<string>(y => filter = y));

            var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 10, 9, 14, 25, 59, TimeSpan.Zero));

            var service = new InboxService(
                new InboxRepository(metadataStore, timeProvider, Options.Create(new InboxRepositoryOptions { EarliestNumberOfDaysToRetrieveMessages = 30 })),
                Substitute.For<IStorageClient>(),
                NullLogger<InboxService>.Instance);

            await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await service.GetAllInboxMessagesAsync(new PagingArguments
                {
                    PageSize = ITEMS_PER_PAGE
                }));

            var earliestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().Date .AddDays(30).Ticks;
            var latestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().AddDays(1).Ticks;
            Assert.DoesNotContain("IsActive", filter);
        }

        [Fact]
        public async Task CallsTableStorageWithActiveOnlyFilterWhenNextPageTokenPassedIn()
        {
            var metadataStore = new MockMetadataClient(new List<MetaMessageEnvelope>()).BuildSinglePageResultsTableClient();
            var filter = "";
            metadataStore.QueryAsync(Arg.Do<string>(y => filter = y));

            var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 10, 9, 14, 25, 59, TimeSpan.Zero));

            var service = new InboxService(
                new InboxRepository(metadataStore, timeProvider, Options.Create(new InboxRepositoryOptions { EarliestNumberOfDaysToRetrieveMessages = 30 })),
                Substitute.For<IStorageClient>(),
                NullLogger<InboxService>.Instance);

            await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await service.GetAllInboxMessagesAsync(new PagingArguments
                {
                    PageSize = ITEMS_PER_PAGE,
                    ActiveOnly = true,
                    NextPageToken = "1234"
                }));

            var earliestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().Date .AddDays(30).Ticks;
            var latestDateTicks = DateTime.MaxValue.Ticks - timeProvider.GetUtcNow().AddDays(1).Ticks;
            Assert.Contains("IsActive eq true", filter);
        }

        [Fact]
        public async Task CanTranslateReturnedInboxMessages()
        {
            var messages = new List<MetaMessageEnvelope> {new MetaMessageEnvelope()};
            var metadataStore = new MockMetadataClient(messages).BuildSinglePageResultsTableClient();

            var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 10, 9, 14, 25, 59, TimeSpan.Zero));

            var service = new InboxService(
                new InboxRepository(metadataStore, timeProvider, Options.Create(new InboxRepositoryOptions { EarliestNumberOfDaysToRetrieveMessages = 30 })),
                Substitute.For<IStorageClient>(),
                NullLogger<InboxService>.Instance);

            var results = await ImplicitElkContext.WithCurrentAsync(
                _elkContext,
                async () => await service.GetAllInboxMessagesAsync(new PagingArguments {PageSize = ITEMS_PER_PAGE}));

            Assert.Equal(messages[0].Id, results.Items[0].Id);
            Assert.Equal(messages[0].BranchId, results.Items[0].BranchId);
            Assert.Equal(messages[0].MessageType, results.Items[0].MessageType);
            Assert.Equal(messages[0].PACode, results.Items[0].PACode);
            Assert.Equal(messages[0].IsActive, results.Items[0].IsActive);
            Assert.Equal(messages[0].IsPrinted, results.Items[0].IsPrinted);
            Assert.Equal(messages[0].IsRead, results.Items[0].IsRead);
            Assert.Equal(messages[0].Comments, results.Items[0].Comments);
            Assert.Equal(messages[0].CreatedDate, results.Items[0].CreatedDate);
            Assert.Equal(messages[0].Errors, results.Items[0].Errors);
            Assert.Equal(messages[0].FileName, results.Items[0].FileName);
            Assert.Equal(messages[0].FileType, results.Items[0].FileType);
            Assert.Equal(messages[0].SourceId, results.Items[0].SourceId);
            Assert.Equal(messages[0].Success, results.Items[0].Success);
            Assert.Equal(messages[0].Oem, results.Items[0].Oem);
        }
    }
}