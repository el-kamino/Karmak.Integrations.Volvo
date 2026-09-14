namespace Karmak.Integrations.Volvo.Inbox.Models.Dto;

public class InboxMessageUpdateArguments
{
    public string? AccountNumber { get; set; }
    public string? Id { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsRead { get; set; }
    public bool? IsPrinted { get; set; }
}