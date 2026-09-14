namespace Karmak.Integrations.Volvo.Inbox.Models
{
    public interface IMetaMessageEnvelope
    {
        string AccountNumber { get; set; }
        string BranchId { get; set; }
        string Comments { get; set; }
        string CorrelationId { get; set; }
        DateTime CreatedDate { get; set; }
        string Errors { get; set; }
        string FileName { get; set; }
        string FileType { get; set; }
        string Id { get; set; }
        bool IsActive { get; set; }
        bool IsPrinted { get; set; }
        bool IsRead { get; set; }
        int MessageTypeId { get; set; }
        string Oem { get; set; }
        string PACode { get; set; }
        string SourceId { get; set; }
        bool? Success { get; set; }
        DateTime SystemCreatedDate { get; set; }
        DateTime SystemLastModifiedDate { get; set; }
    }
}