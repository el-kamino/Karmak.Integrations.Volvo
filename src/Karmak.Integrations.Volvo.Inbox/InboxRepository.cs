using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Karmak.Integrations.Volvo.Inbox.Storage.Table;
using Karmak.Integrations.Volvo.Inbox.Storage.Table.Filtering;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Inbox
{
    public class InboxRepository : IInboxRepository
    {
        private readonly IMetaDataStore<MetaMessageEnvelope> _dataStore;
        private readonly TimeProvider _timeProvider;
        private readonly InboxRepositoryOptions _options;

        public InboxRepository(IMetaDataStore<MetaMessageEnvelope> dataStore, TimeProvider timeProvider, IOptions<InboxRepositoryOptions> options)
        {
            _dataStore = dataStore;
            _timeProvider = timeProvider;
            _options = options.Value;
        }

        public async Task<MetaMessageEnvelope> UpsertAsync(MetaMessageEnvelope entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = BuildRowKey(entity.MessageType, entity.CreatedDate, entity.BranchId, entity.Oem);
            }

            return await _dataStore.CreateAsync(entity);
        }

        public async Task<PagedResult<InboxMessage>> GetAllAsync((string, string) accountAndBranchNumber, PagingArguments pagingArguments)
        {
            var (accountNumber, branchId) = accountAndBranchNumber;

            var (rangeStart, rangeEnd) = GetRangeInTicks(InboxMessageType.FixedWidthFile, pagingArguments.Oem);

            var queryArgs = new QueryArguments(pagingArguments)
            {
                AccountNumber = accountNumber,
                BranchId = branchId,
                StartIdRange = rangeStart,
                EndIdRange = rangeEnd
            };

            if (pagingArguments.ActiveOnly)
            {
                queryArgs.FilterConditions = new List<FilterCondition> { BuildIsActiveFilter() };
            }

            string filter = FilterBuilder.BuildFilter(queryArgs);
            List<MetaMessageEnvelope> results = await _dataStore.QueryAsync(filter);
            var pagedResult = new PagedResultBuilder<MetaMessageEnvelope>(results).Build(queryArgs);

            return BuildPagedResult(pagedResult, string.IsNullOrWhiteSpace(pagingArguments.NextPageToken));
        }

        public async Task<MetaMessageEnvelope> GetByIdAsync(string accountNumber, string id)
        {
            return await _dataStore.GetByIdAsync(accountNumber, id);
        }

        public async Task<List<MessageContentPointer>> GetMessageContentPointersAsync(string accountNumber,
            string branchId, List<string> ids)
        {
            var messages = await GetBulkByIdAsync(accountNumber, branchId, ids);
            return messages
                .Select(m => new MessageContentPointer { MessageId = m.Id, ContentUri = m.ContentUri })
                .ToList();
        }

        public async Task<InboxMessage> UpdateAsync(InboxMessageUpdateArguments args)
        {
            var envelope = await _dataStore.GetByIdAsync(args.AccountNumber, args.Id);

            SetUpdatedFields(envelope, args);

            return (await _dataStore.UpdateAsync(envelope)).ToInboxMessage();
        }

        public async Task<List<InboxMessage>> BulkUpdateAsync(string accountNumber, string branchId,
           BulkMessageUpdateArguments bulkArgs)
        {
            var envelopes = 
                await GetBulkByIdAsync(accountNumber, branchId, bulkArgs.Messages.Select(m => m.Id).ToList());

            foreach (var args in bulkArgs.Messages)
            {
                var envelope = envelopes.First(e => e.Id == args.Id);
                SetUpdatedFields(envelope, args);
            }

            var updatedMessages = await _dataStore.BulkUpdateAsync(accountNumber, envelopes);

            return updatedMessages.Select(m => m.ToInboxMessage()).ToList();
        }

        private async Task<List<MetaMessageEnvelope>> GetBulkByIdAsync(string accountNumber, string branchId, List<string> ids)
        {
            if (ids.Count <= 1)
            {
                var result = await _dataStore.GetByIdAsync(accountNumber, ids[0]);
                return new List<MetaMessageEnvelope> { result };
            }

            ids.Sort();
            var lowestId = ids.First();
            var highestId = ids.Last();

            var queryArgs = new QueryArguments
            {
                AccountNumber = accountNumber,
                BranchId = branchId,
                StartIdRange = lowestId,
                EndIdRange = highestId,
                ItemsPerPage = 1000,
                PageNumber = 1
            };

            string filter = FilterBuilder.BuildFilter(queryArgs, true);

            List<MetaMessageEnvelope> results = await _dataStore.QueryAsync(filter);
            var pagedResult = new PagedResultBuilder<MetaMessageEnvelope>(results).Build(queryArgs);

            if (pagedResult.Items == null)
                return new List<MetaMessageEnvelope>();

            return pagedResult.Items.Where(i => ids.Contains(i.RowKey)).ToList();
        }

        private static string BuildSearchKey(InboxMessageType messageType, long ticks, string? oem)
        {
            var key = $"{MetaMessageEnvelope.MessageTypePrefix}{(int)messageType}|{MetaMessageEnvelope.SystemCreatedDatePrefix}{ticks}|";

            if (!string.IsNullOrWhiteSpace(oem))
            {
                key = $"{key}{MetaMessageEnvelope.OemPrefix}{oem.ToUpper()}|";
            }

            return key;
        }

        private static string BuildRowKey(InboxMessageType messageType, DateTime createdDate, string? branchId, string? oem)
        {
            var ticks = DateTime.MaxValue.Ticks - createdDate.Ticks;

            if (string.IsNullOrWhiteSpace(branchId))
            {
                return $"{BuildSearchKey(messageType, ticks, oem)}Uid:{Guid.NewGuid()}";
            }

            return $"{BuildSearchKey(messageType, ticks, oem)}{MetaMessageEnvelope.BranchPrefix}{branchId}|Uid:{Guid.NewGuid()}";
        }

        private (string, string) GetRangeInTicks(InboxMessageType messageType, string oem)
        {
            // NOTE: These ranges work counter-intuitively, and really should be built the way the are with earliest ticks being the ending range
            var earliestDateTicks = _timeProvider.CalculateEarliestTicks(_options.EarliestNumberOfDaysToRetrieveMessages * -1);
            var latestDateTicks = _timeProvider.CalculateLatestTicks();
            var rangeStart = BuildSearchKey(messageType, latestDateTicks, oem);
            var rangeEnd = BuildSearchKey(messageType, earliestDateTicks, oem);
            return (rangeStart, rangeEnd);
        }

        private static FilterCondition BuildIsActiveFilter()
        {
            return new FilterCondition
            {
                Operator = FilterOperators.And,
                PropertyName = nameof(MetaMessageEnvelope.IsActive),
                Comparison = FilterComparisons.Equal,
                ComparedValue = "true",
                ComparedValueIsString = false
            };
        }

        private static PagedResult<InboxMessage> BuildPagedResult(PagedResult<MetaMessageEnvelope> result,
           bool firstPage = false)
        {
            return new PagedResult<InboxMessage>
            {
                CurrentPage = firstPage ? result.CurrentPage : null,
                HasMoreItems = result.HasMoreItems,
                ItemsPerPage = result.ItemsPerPage,
                NextPageToken = result.NextPageToken,
                Items = result.Items.Select(i => i.ToInboxMessage()).ToList()
            };
        }

        private static void SetUpdatedFields(MetaMessageEnvelope envelope, InboxMessageUpdateArguments args)
        {
            if (args.IsActive != null)
                envelope.IsActive = args.IsActive.Value;

            if (args.IsRead != null)
                envelope.IsRead = args.IsRead.Value;

            if (args.IsPrinted != null)
                envelope.IsPrinted = args.IsPrinted.Value;
        }
    }
}