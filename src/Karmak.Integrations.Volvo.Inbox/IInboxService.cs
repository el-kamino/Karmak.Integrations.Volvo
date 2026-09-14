using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;

namespace Karmak.Integrations.Volvo.Inbox;

public interface IInboxService
{
    Task<MetaMessageEnvelope> AddFixedWidthFileAsync(FixedWidthFile file);
    Task<PagedResult<InboxMessage>> GetAllInboxMessagesAsync(PagingArguments args);
    Task<InboxMessageContents> GetMessageContentsAsync(string messageId);
    Task<List<InboxMessageContents>> GetBulkMessageContentsAsync(BulkRetrieveMessageContentsArgs args);
    Task<InboxMessage> UpdateInboxMessageAsync(InboxMessageUpdateArguments args);
    Task<List<InboxMessage>> UpdateInboxMessagesAsync(BulkMessageUpdateArguments args);
}
