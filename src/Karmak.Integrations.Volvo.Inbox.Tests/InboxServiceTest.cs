using Azure;
using FluentValidation;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Inbox;
using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Karmak.Integrations.Volvo.Inbox.Storage.Table;
using Karmak.Integrations.Volvo.Inbox.Validators.Blob;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Integrations.Inbox.Core.Test
{
    public class InboxServiceTest
    {
        private const string PA_CODE = "12345";
        private const string FAKE_CONTENT_URL = "http://fakefilecontent";
        private const string ACCOUNT_ID = "a0d9e6b9-c2e6-4035-a6bc-260cbe54e4b5";
        private const string INSTANCE_ID = "1e6841c0-25d3-488c-a2df-2f0920149c39";
        private const string BRANCH_ID = "a3f79084-de28-431e-af74-578f07fa0e1c";
        private const string USER_ID = "f6a1c40b-3606-49fb-8931-7864609394b9";
        private static readonly string SAMPLE_CORRELATION_ID = $"kmk:{PA_CODE}|id:c8d54056-f8fa-4d50-a415-4be7eca6a1f7";
        private InboxService _service;

        private IMetaDataStore<MetaMessageEnvelope> _metaDataStore;
        private List<MetaMessageEnvelope> _mockMessages;
        private const int ITEMS_PER_PAGE = 2;
        private ElkContext _elkContext;

        private void InitializeTest(Func<IMetaDataStore<MetaMessageEnvelope>> mockMetaDataStoreBuilder, Func<ElkContext> elkContextBuilder = null)
        {
            _mockMessages = BuildMockMessages();
            _elkContext = elkContextBuilder != null ? elkContextBuilder.Invoke() : BuildFakeElkContext();

            _metaDataStore = mockMetaDataStoreBuilder.Invoke();

            var inboxRepository = new InboxRepository(
                _metaDataStore, TimeProvider.System, Options.Create(new InboxRepositoryOptions { EarliestNumberOfDaysToRetrieveMessages = 30 }));

            var blobStorageClient = Substitute.For<IStorageClient>();
            blobStorageClient.DownloadAsync<List<FixedWidthLine>>(Arg.Any<string>())
                .Returns(Task.FromResult(new List<FixedWidthLine>
                {
                    new FixedWidthLine {LineNumber = 1, Contents = "Contents1"},
                    new FixedWidthLine {LineNumber = 2, Contents = "Contents2"}
                }));

            blobStorageClient.UploadAsync(Arg.Any<BlobUploadRequest<List<FixedWidthLine>>>())
                .Returns(Task.FromResult(FAKE_CONTENT_URL));

            blobStorageClient.UploadAsync(Arg.Any<BlobUploadRequest<string>>())
                .Returns(Task.FromResult(FAKE_CONTENT_URL));

            _service = new InboxService(inboxRepository, blobStorageClient, NullLogger<InboxService>.Instance);
        }

        [Theory]
        [InlineData(ModelConstants.OEM_NAME_VOLVO)]
        [InlineData("Volvo")]
        [InlineData("volvo")]
        public async Task GetAllInboxMessagesOnlyRetrievesMessagesForSpecifiedOemIfPassedIn(string oem)
        {
            InitializeTest(() => BuildMockMetadataStore(filter =>
            {
                var messages = _mockMessages.Where(m => m.MessageType == InboxMessageType.FixedWidthFile && m.Oem == oem).ToList();

                return BuildMockResult(messages);
            }));

            var volvoMessage = BuildFakeInboxMessage(InboxMessageType.FixedWidthFile, "11");
            volvoMessage.Oem = oem;
            _mockMessages.Add(volvoMessage);

            var gmMessage = BuildFakeInboxMessage(InboxMessageType.FixedWidthFile, "12");
            gmMessage.Oem = "GM";
            _mockMessages.Add(gmMessage);

            var args = new PagingArguments { PageSize = ITEMS_PER_PAGE, Oem = ModelConstants.OEM_NAME_VOLVO };
            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));
            Assert.Single(results.Items);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetAllInboxMessagesAsyncFailsForMissingInvalidPageSize(int pageSize)
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.GetAllInboxMessagesAsync(new PagingArguments { PageSize = pageSize })));
        }

        [Theory]
        [InlineData(false, null, null)]
        [InlineData(null, true, null)]
        [InlineData(null, null, true)]
        [InlineData(false, true, true)]
        public async Task CanUpdateInboxMessageAsExpected(bool? isActive, bool? isRead, bool? isPrinted)
        {
            var metadataStore = BuildMockMetadataStore(_ => BuildMockResult(_mockMessages));
            metadataStore.GetByIdAsync(Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(ci =>
                {
                    var id = ci.ArgAt<string>(1);
                    return _mockMessages.First(m => m.Id == id);
                });

            metadataStore.UpdateAsync(Arg.Any<MetaMessageEnvelope>())
                .ReturnsForAnyArgs(ci => Task.FromResult(ci.ArgAt<MetaMessageEnvelope>(0)));

            InitializeTest(() => metadataStore);

            var args = new InboxMessageUpdateArguments
            {
                Id = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|2",
                IsActive = isActive,
                IsRead = isRead,
                IsPrinted = isPrinted
            };

            var savedMessage = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.UpdateInboxMessageAsync(args));

            Assert.NotNull(savedMessage);

            if (isActive != null)
                Assert.Equal(isActive.Value, savedMessage.IsActive);

            if (isRead != null)
                Assert.Equal(isRead.Value, savedMessage.IsRead);

            if (isPrinted != null)
                Assert.Equal(isPrinted.Value, savedMessage.IsPrinted);
        }

        [Fact]
        public async Task AddFixedWidthFileAsyncFailsForEmptyLineItemsIfNotAClaimCheck()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.AddFixedWidthFileAsync(new FixedWidthFile
                    {
                        LineItems = new List<FixedWidthLine>(),
                        CreatedDate = new DateTime(2020, 2, 1),
                    })));
        }

        [Fact]
        public async Task AddFixedWidthFileAsyncFailsForMissingCreatedDate()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.AddFixedWidthFileAsync(
                        new FixedWidthFile
                        {
                            CreatedDate = default(DateTime),
                            LineItems = new List<FixedWidthLine> { new FixedWidthLine() }
                        })));
        }

        [Fact]
        public async Task AddFixedWidthFileAsyncFailsForMissingLineItems()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.AddFixedWidthFileAsync(new FixedWidthFile())));
        }

        [Fact]
        public async Task AddFixedWidthFileAsyncFailsForNullMessage()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext, () => _service.AddFixedWidthFileAsync(null)));
        }

        [Fact]
        public async Task AddItemToAndRetrieveIt_Inbox_FixedWidthFile_HandlesMissingBranchId()
        {
            var metadataClient = BuildMockMetadataStore(_ => BuildMockResult(_mockMessages));
            metadataClient.CreateAsync(Arg.Any<MetaMessageEnvelope>())
                .ReturnsForAnyArgs(ci =>
                {
                    var addedEnvelop = ci.Arg<MetaMessageEnvelope>();
                    _mockMessages.Add(addedEnvelop);

                    return Task.FromResult(addedEnvelop);
                });

            InitializeTest(() => metadataClient, () => BuildFakeElkContext(branchId: null));

            var fixedWidthFile = new FixedWidthFile
            {
                CreatedDate = DateTime.MaxValue,
                LineItems = new List<FixedWidthLine>
                {
                   new FixedWidthLine{ LineNumber = 1},
                   new FixedWidthLine{ LineNumber = 2}
                }
            };

            var addedItem = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.AddFixedWidthFileAsync(fixedWidthFile));

            Assert.Equal(INSTANCE_ID.ToLower(), addedItem.AccountNumber.ToLower());
            Assert.Null(addedItem.BranchId);
            Assert.DoesNotContain(MetaMessageEnvelope.BranchPrefix, addedItem.Id);
        }

        [Fact]
        public async Task AddItemToAndRetrieveIt_Inbox_FixedWidthFile_HandlesMissingInstanceId()
        {
            var metadataClient = BuildMockMetadataStore(_ => BuildMockResult(_mockMessages));
            metadataClient.CreateAsync(Arg.Any<MetaMessageEnvelope>())
                .ReturnsForAnyArgs(ci =>
                {
                    var addedEnvelop = ci.Arg<MetaMessageEnvelope>();
                    _mockMessages.Add(addedEnvelop);

                    return Task.FromResult(addedEnvelop);
                });

            InitializeTest(() => metadataClient, () => BuildFakeElkContext(instanceId: null));

            var fixedWidthFile = new FixedWidthFile
            {
                CreatedDate = new DateTime(2024, 5, 19),
                LineItems = new List<FixedWidthLine>
                {
                   new FixedWidthLine{ LineNumber = 1},
                   new FixedWidthLine{ LineNumber = 2}
                }
            };

            var addedItem = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.AddFixedWidthFileAsync(fixedWidthFile));

            Assert.Equal(ACCOUNT_ID.ToLower(), addedItem.AccountNumber.ToLower());
        }

        [Fact]
        public async Task BulkGetMessageContentsFailsForEmptyIdInMessageIds()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.GetBulkMessageContentsAsync(new BulkRetrieveMessageContentsArgs
                    { MessageIds = new List<string> { "1", "", "2" } })));
        }

        [Fact]
        public async Task BulkGetMessageContentsFailsForEmptyMessageIds()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.GetBulkMessageContentsAsync(new BulkRetrieveMessageContentsArgs
                    { MessageIds = new List<string>() })));
        }

        [Fact]
        public async Task BulkGetMessageContentsFailsForNullArgs()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.GetBulkMessageContentsAsync(null)));
        }

        [Fact]
        public async Task BulkGetMessageContentsFailsForNullIdInMessageIds()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.GetBulkMessageContentsAsync(new BulkRetrieveMessageContentsArgs
                    { MessageIds = new List<string> { "1", null, "2" } })));
        }

        [Fact]
        public async Task BulkGetMessageContentsFailsForNullMessageIds()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.GetBulkMessageContentsAsync(new BulkRetrieveMessageContentsArgs())));
        }

        [Fact]
        public async Task BulkUpdateFailsWhenArgsHaveNoMessages()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.UpdateInboxMessagesAsync(new BulkMessageUpdateArguments
                    { Messages = new List<InboxMessageUpdateArguments>() })));
        }

        [Fact]
        public async Task BulkUpdateFailsWhenArgsHaveNullMessagesProperty()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.UpdateInboxMessagesAsync(new BulkMessageUpdateArguments())));
        }

        [Fact]
        public async Task BulkUpdateFailsWhenNoArgsArePassedIn()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext, () => _service.UpdateInboxMessagesAsync(null)));
        }

        [Fact]
        public async Task CanAddItemToAndRetrieveIt_Inbox_FixedWidthFile()
        {
            var metadataClient = BuildMockMetadataStore(_ => BuildMockResult(_mockMessages));
            metadataClient.CreateAsync(Arg.Any<MetaMessageEnvelope>())
                .ReturnsForAnyArgs(ci =>
                {
                    var addedEnvelop = ci.Arg<MetaMessageEnvelope>();
                    _mockMessages.Add(addedEnvelop);

                    return Task.FromResult(addedEnvelop);
                });

            InitializeTest(() => metadataClient);

            var fixedWidthFile = new FixedWidthFile
            {
                CreatedDate = new DateTime(2025, 10, 8),
                PACode = PA_CODE,
                FileName = "FileName",
                FileType = "FileType",
                SourceId = "SourceId",
                LineItems = new List<FixedWidthLine>
                {
                    new FixedWidthLine { LineNumber = 1 },
                    new FixedWidthLine { LineNumber = 2}
                }
            };

            var addedItem = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.AddFixedWidthFileAsync(fixedWidthFile));

            Assert.Equal(INSTANCE_ID.ToLower(), addedItem.AccountNumber.ToLower());
            Assert.Equal(BRANCH_ID.ToLower(), addedItem.BranchId.ToLower());
            Assert.Equal(fixedWidthFile.PACode, addedItem.PACode.ToLower());
            Assert.True(addedItem.IsActive);
            Assert.Equal(fixedWidthFile.FileName, addedItem.FileName);
            Assert.Equal(fixedWidthFile.FileType, addedItem.FileType);
            Assert.Equal(fixedWidthFile.SourceId, addedItem.SourceId);
            Assert.Equal(fixedWidthFile.FileDescription, addedItem.FileDescription);
            Assert.Contains($"{MetaMessageEnvelope.BranchPrefix}{BRANCH_ID.ToLower()}", addedItem.Id);
            Assert.Contains($"{MetaMessageEnvelope.OemPrefix}{ModelConstants.OEM_NAME_VOLVO}", addedItem.Id);

            var args = new PagingArguments { PageSize = _mockMessages.Count };
            var allMessages = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));
            Assert.Contains(allMessages.Items, i => i.Id == addedItem.Id);
        }

        [Fact]
        public async Task CanBulkGetMessageContents()
        {
            var messageId2 = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|2";
            var messageId3 = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|3";

            InitializeTest(() => BuildMockMetadataStore(filter =>
                {
                    Assert.True(filter.Contains(messageId2));
                    Assert.True(filter.Contains(messageId3));
                    var messages = _mockMessages.Where(m => m.Id == messageId2 || m.Id == messageId3).ToList();
                    return BuildMockResult(messages);
                }
            ));

            var args = new BulkRetrieveMessageContentsArgs
            {
                MessageIds = new List<string> { messageId2, messageId3 }
            };

            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetBulkMessageContentsAsync(args));
            Assert.NotNull(results);

            Assert.Contains(results, r => r.Id == messageId2 && r.Lines.Any());
            Assert.Contains(results, r => r.Id == messageId3 && r.Lines.Any());
        }

        [Fact]
        public async Task CanBulkUpdateMultipleMessages()
        {
            var message2Id = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|2";
            var message4Id = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|4";
            var message7Id = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|7";

            var messagesToSelect = new[] { message2Id, message4Id, message7Id };

            InitializeTest(() =>
            {
                var store = BuildMockMetadataStore(filter =>
                {
                    var messages = _mockMessages.Where(m => messagesToSelect.Contains(m.Id)).ToList();
                    return BuildMockResult(messages);
                });

                store.BulkUpdateAsync(Arg.Any<string>(), Arg.Any<List<MetaMessageEnvelope>>())
                    .ReturnsForAnyArgs(ci => Task.FromResult(ci.ArgAt<List<MetaMessageEnvelope>>(1)));

                return store;
            });

            var args = new BulkMessageUpdateArguments
            {
                Messages = new List<InboxMessageUpdateArguments>
                {
                    new InboxMessageUpdateArguments { Id = message2Id, IsPrinted = true },
                    new InboxMessageUpdateArguments { Id = message4Id, IsRead = true },
                    new InboxMessageUpdateArguments { Id = message7Id, IsActive = false }
                }
            };

            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.UpdateInboxMessagesAsync(args));

            Assert.Contains(results, m => m.Id == message2Id && m.IsPrinted);
            Assert.Contains(results, m => m.Id == message4Id && m.IsRead);
            Assert.Contains(results, m => m.Id == message7Id && !m.IsActive);

            // Make sure the underlying store is appropriately updated
            Assert.Contains(_mockMessages, m => m.Id == message2Id && m.IsPrinted);
            Assert.Contains(_mockMessages, m => m.Id == message4Id && m.IsRead);
            Assert.Contains(_mockMessages, m => m.Id == message7Id && !m.IsActive);
        }

        [Fact]
        public async Task CanBulkUpdateSingleMessage()
        {
            var metadataClient = BuildMockMetadataStore(_ => BuildMockResult(_mockMessages));
            metadataClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(ci =>
                {
                    var id = ci.ArgAt<string>(1);
                    return _mockMessages.First(m => m.Id == id);
                });

            metadataClient.BulkUpdateAsync(Arg.Any<string>(), Arg.Any<List<MetaMessageEnvelope>>())
                .ReturnsForAnyArgs(ci => Task.FromResult(ci.ArgAt<List<MetaMessageEnvelope>>(1)));

            InitializeTest(() => metadataClient);

            var messageId = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|2";

            var args = new BulkMessageUpdateArguments
            {
                Messages = new List<InboxMessageUpdateArguments>
                {
                    new InboxMessageUpdateArguments {Id = messageId, IsPrinted = true}
                }
            };

            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.UpdateInboxMessagesAsync(args));

            Assert.Contains(results, m => m.Id == messageId && m.IsPrinted);

            // Make sure the underlying store is appropriately updated
            Assert.Contains(_mockMessages, m => m.Id == messageId && m.IsPrinted);
        }

        [Fact]
        public async Task CanGetAllInboxMessages()
        {
            InitializeTest(() => BuildMockMetadataStore(_ => BuildMockResult(_mockMessages.Where(m => m.MessageType == InboxMessageType.FixedWidthFile).ToList())));
            var args = new PagingArguments { PageSize = ITEMS_PER_PAGE };
            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));
            Assert.Contains(results.Items, i => i.MessageType == InboxMessageType.FixedWidthFile);
            Assert.DoesNotContain(results.Items, i => i.MessageType == InboxMessageType.SendVolvoFileResponse);
            Assert.True(results.Items.All(i => !string.IsNullOrWhiteSpace(i.BranchId)));
            Assert.True(results.Items.All(i => !string.IsNullOrWhiteSpace(i.PACode)));
            Assert.True(results.Items.All(i => i.IsActive));
            Assert.True(results.Items.All(i => !string.IsNullOrWhiteSpace(i.Id)));
            Assert.True(results.Items.All(i => !string.IsNullOrWhiteSpace(i.FileName)));
            Assert.True(results.Items.Where(i => i.MessageType == InboxMessageType.FixedWidthFile)
                .All(i => !string.IsNullOrWhiteSpace(i.SourceId)));
        }

        [Fact]
        public async Task CanGetAllInboxMessagesSortedByFileNameAscending()
        {
            InitializeTest(() => BuildMockMetadataStore(_ => BuildMockResult(_mockMessages.Where(m => m.MessageType == InboxMessageType.FixedWidthFile).ToList())));
            var args = new PagingArguments { PageSize = ITEMS_PER_PAGE, SortField = nameof(InboxMessage.FileName), SortAscending = true };
            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));

            Assert.Equal("F10", results.Items.First().FileName);
            Assert.Equal("F2", results.Items.Last().FileName);

            var index = 3;

            while (results.HasMoreItems)
            {
                args.NextPageToken = results.NextPageToken;

                results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    async () => await _service.GetAllInboxMessagesAsync(args));

                foreach (var item in results.Items)
                {
                    Assert.Equal($"F{index++}", item.FileName);
                }
            }
        }

        [Fact]
        public async Task CanGetAllInboxMessagesSortedByFileNameDescending()
        {
            InitializeTest(() => BuildMockMetadataStore(_ => BuildMockResult(_mockMessages.Where(m => m.MessageType == InboxMessageType.FixedWidthFile).ToList())));
            var args = new PagingArguments { PageSize = ITEMS_PER_PAGE, SortField = nameof(InboxMessage.FileName), SortAscending = false };
            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));

            Assert.Equal("F9", results.Items.First().FileName);
            Assert.Equal("F8", results.Items.Last().FileName);

            args.NextPageToken = results.NextPageToken;

            results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));

            Assert.Equal("F7", results.Items.First().FileName);
            Assert.Equal("F6", results.Items.Last().FileName);
        }

        [Fact]
        public async Task CanGetAllInboxMessagesSortedByFileTypeAscending()
        {
            InitializeTest(() => BuildMockMetadataStore(_ => BuildMockResult(_mockMessages.Where(m => m.MessageType == InboxMessageType.FixedWidthFile).ToList())));
            var args = new PagingArguments { PageSize = 10, SortField = nameof(InboxMessage.FileType), SortAscending = true };
            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));

            Assert.Equal("F150", results.Items.First().FileType);
            Assert.Equal("M350", results.Items.Last().FileType);
        }

        [Fact]
        public async Task CanGetAllInboxMessagesSortedByFileTypeDescending()
        {
            InitializeTest(() => BuildMockMetadataStore(_ => BuildMockResult(_mockMessages.Where(m => m.MessageType == InboxMessageType.FixedWidthFile).ToList())));
            var args = new PagingArguments { PageSize = 10, SortField = nameof(InboxMessage.FileType), SortAscending = false };
            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));

            Assert.Equal("M350", results.Items.First().FileType);
            Assert.Equal("F150", results.Items.Last().FileType);
        }

        [Fact]
        public async Task CanGetAllInboxMessagesSortedByCreatedDateAscending()
        {
            InitializeTest(() => BuildMockMetadataStore(_ => BuildMockResult(_mockMessages.Where(m => m.MessageType == InboxMessageType.FixedWidthFile).ToList())));
            var args = new PagingArguments { PageSize = 10, SortField = nameof(InboxMessage.CreatedDate), SortAscending = true };
            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));

            Assert.Equal(DateTime.Today.AddDays(-3), results.Items.First().CreatedDate);
            Assert.Equal(DateTime.Today.AddDays(-1), results.Items.Last().CreatedDate);
        }

        [Fact]
        public async Task CanGetAllInboxMessagesSortedByCreatedDateDescending()
        {
            InitializeTest(() => BuildMockMetadataStore(_ => BuildMockResult(_mockMessages.Where(m => m.MessageType == InboxMessageType.FixedWidthFile).ToList())));
            var args = new PagingArguments { PageSize = 10, SortField = nameof(InboxMessage.CreatedDate), SortAscending = false };
            var results = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetAllInboxMessagesAsync(args));

            Assert.Equal(DateTime.Today.AddDays(-3), results.Items.Last().CreatedDate);
            Assert.Equal(DateTime.Today.AddDays(-1), results.Items.First().CreatedDate);
        }

        [Fact]
        public async Task CanGetMessageContents()
        {
            var metadataClient = BuildMockMetadataStore(_ => BuildMockResult(_mockMessages));
            metadataClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(ci =>
                {
                    var id = ci.ArgAt<string>(1);
                    return _mockMessages.First(m => m.Id == id);
                });

            InitializeTest(() => metadataClient);

            var result = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetMessageContentsAsync(
                    $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|2"));

            Assert.NotNull(result);
            Assert.Equal($"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.FixedWidthFile}|2",
                result.Id);
            Assert.Equal(2, result.Lines.Count());
        }

        [Fact]
        public async Task GetAllInboxMessagesAsyncFailsForMissingInvalidPageNumber()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext,
                    () => _service.GetAllInboxMessagesAsync(new PagingArguments { PageNumber = 0 })));
        }

        [Fact]
        public async Task GetAllInboxMessagesAsyncFailsForNullMessage()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext, () => _service.GetAllInboxMessagesAsync(null)));
        }

        [Fact]
        public async Task GetMessageContentsAsyncFailsForMissingId()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext, () => _service.GetMessageContentsAsync("")));
        }

        [Fact]
        public async Task GetMessageContentsHandlesFilesWithNoContents()
        {
            var metadataClient = BuildMockMetadataStore(_ => BuildMockResult(_mockMessages));
            metadataClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(ci =>
                {
                    var id = ci.ArgAt<string>(1);
                    return _mockMessages.First(m => m.Id == id);
                });

            InitializeTest(() => metadataClient);

            var result = await ImplicitElkContext.WithCurrentAsync(_elkContext,
                async () => await _service.GetMessageContentsAsync(
                    $"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.SendVolvoFileResponse}|1"));
            Assert.NotNull(result);
            Assert.Equal($"{MetaMessageEnvelope.MessageTypePrefix}{(int)InboxMessageType.SendVolvoFileResponse}|1",
                result.Id);
            Assert.Equal(0, result.Lines.Count());
        }

        [Fact]
        public async Task UpdateInboxMessageFailsForMissingAllUpdateFlags()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            var args = new InboxMessageUpdateArguments
            {
                Id = "2"
            };

            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext, () => _service.UpdateInboxMessageAsync(args)));
        }

        [Fact]
        public async Task UpdateInboxMessageFailsForMissingId()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            var args = new InboxMessageUpdateArguments { IsActive = true };

            await Assert.ThrowsAsync<ValidationException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext, () => _service.UpdateInboxMessageAsync(args)));
        }

        [Fact]
        public async Task UpdateInboxMessageFailsForNullMessage()
        {
            InitializeTest(() => Substitute.For<IMetaDataStore<MetaMessageEnvelope>>());
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await ImplicitElkContext.WithCurrentAsync(_elkContext, () => _service.UpdateInboxMessageAsync(null)));
        }

        private static ElkContext BuildFakeElkContext(
            string accountId = ACCOUNT_ID,
            string instanceId = INSTANCE_ID,
            string branchId = BRANCH_ID,
            string userId = USER_ID)
        {
            Func<string, Guid?> convertVal = (val) => !string.IsNullOrWhiteSpace(val) ? Guid.Parse(val) : (Guid?)null;

            var idBuilder = new ElkIdentity.Builder();
            idBuilder.Account = convertVal(accountId);
            idBuilder.User = convertVal(userId);
            ElkIdentity id = idBuilder.Build();

            var appBuilder = new ElkApplicationContext.Builder();
            appBuilder.Instance = convertVal(instanceId);
            appBuilder.Branch = convertVal(branchId);

            return new ElkContext(id, id, appBuilder.Build(), new ElkRole(Guid.Empty));
        }

        private static List<MetaMessageEnvelope> BuildMockMessages()
        {
            return new List<MetaMessageEnvelope>
            {
                BuildFakeInboxMessage(InboxMessageType.SendVolvoFileResponse, "1", SAMPLE_CORRELATION_ID),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"2"),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"3"),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"4"),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"5"),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"6"),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"7"),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"8"),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"9"),
                BuildFakeInboxMessage(InboxMessageType.FixedWidthFile,"10")
            };
        }

        private static MetaMessageEnvelope BuildFakeInboxMessage(InboxMessageType messageType, string fileId, string correlationId = null)
        {
            string fileType;
            DateTime createdDate;

            switch (int.Parse(fileId) % 3)
            {
                case 0:
                    fileType = "F150";
                    createdDate = DateTime.Today.AddDays(-1);
                    break;
                case 1:
                    fileType = "L250";
                    createdDate = DateTime.Today.AddDays(-3);
                    break;
                default:
                    fileType = "M350";
                    createdDate = DateTime.Today.AddDays(-2);
                    break;
            }

            var msg = new MetaMessageEnvelope
            {
                ETag = ETag.All,
                ContentUri = "test",
                AccountNumber = INSTANCE_ID.ToString(),
                BranchId = BRANCH_ID.ToString(),
                PACode = PA_CODE,
                Id = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)messageType}|{fileId}",
                FileName = $"F{fileId}",
                FileType = fileType,
                CreatedDate = createdDate,
                MessageType = messageType,
                IsActive = true,
                SourceId = "test"
            };


            if (messageType != InboxMessageType.FixedWidthFile)
            {
                msg.ContentUri = null;
            }

            return msg;
        }

        private IMetaDataStore<MetaMessageEnvelope> BuildMockMetadataStore(Func<string, List<MetaMessageEnvelope>> expression)
        {
            var metadataStore = Substitute.For<IMetaDataStore<MetaMessageEnvelope>>();
            metadataStore.QueryAsync(Arg.Any<string>()).ReturnsForAnyArgs(ci =>
            {
                var filter = ci.ArgAt<string>(0);
                return expression.Invoke(filter);
            });

            return metadataStore;
        }

        private List<MetaMessageEnvelope> BuildMockResult(List<MetaMessageEnvelope> messages)
        {
            return messages;
        }
    }
}