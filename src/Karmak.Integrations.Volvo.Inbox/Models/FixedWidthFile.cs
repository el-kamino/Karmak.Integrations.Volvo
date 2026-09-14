namespace Karmak.Integrations.Volvo.Inbox.Models;

public class FixedWidthFile
{
    public string? BranchId { get; set; }
    public string? PACode { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public IEnumerable<FixedWidthLine>? LineItems { get; set; }
    public string? SourceId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? FileDescription { get; set; }
}