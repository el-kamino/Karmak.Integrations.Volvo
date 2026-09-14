using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;

namespace Karmak.Integrations.Volvo.Inbox;

public interface IInboxRepository
{
    Task<MetaMessageEnvelope> UpsertAsync(MetaMessageEnvelope entity);
    Task<PagedResult<InboxMessage>> GetAllAsync((string, string) accountAndBranchNumber, PagingArguments pagingArguments);
    Task<MetaMessageEnvelope> GetByIdAsync(string accountNumber, string id);
    Task<List<MessageContentPointer>> GetMessageContentPointersAsync(string accountNumber, string branchId, List<string> ids);
    Task<InboxMessage> UpdateAsync(InboxMessageUpdateArguments args);
    Task<List<InboxMessage>> BulkUpdateAsync(string accountNumber, string branchId, BulkMessageUpdateArguments bulkArgs);
}