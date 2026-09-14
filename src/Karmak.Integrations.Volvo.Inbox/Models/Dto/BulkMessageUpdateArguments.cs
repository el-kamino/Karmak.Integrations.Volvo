namespace Karmak.Integrations.Volvo.Inbox.Models.Dto;

public class BulkMessageUpdateArguments
{
    public List<InboxMessageUpdateArguments>? Messages { get; set; }
}