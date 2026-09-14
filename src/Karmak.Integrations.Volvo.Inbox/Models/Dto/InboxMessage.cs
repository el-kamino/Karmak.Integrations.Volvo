using System.Diagnostics;

namespace Karmak.Integrations.Volvo.Inbox.Models.Dto;

[DebuggerDisplay("Id = {Id}, BranchId = {BranchId}, MessageType = {MessageType}, IsActive = {IsActive}")]
public class InboxMessage
{
    public string Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string BranchId { get; set; }
    public string PACode { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
    public bool? Success { get; set; }
    public string Errors { get; set; }
    public string Comments { get; set; }
    public bool IsActive { get; set; }
    public bool IsRead { get; set; }
    public bool IsPrinted { get; set; }
    public string SourceId { get; set; }
    public string Oem { get; set; }
    public InboxMessageType? MessageType { get; set; }
    public string FileDescription { get; set; }
}