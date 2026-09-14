namespace Karmak.Integrations.Volvo.Inbox.Models.Dto;

public class InboxMessageContents
{
    public string Id { get; set; }
    public IEnumerable<InboxMessageContentLine> Lines { get; set; }
}