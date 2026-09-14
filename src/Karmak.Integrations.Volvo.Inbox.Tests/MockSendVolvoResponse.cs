using System;

namespace Integrations.Inbox.Core.Test;

public class MockSendVolvoResponse
{
    public string CorrelationId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string AccountNumber { get; set; }
    public string BranchId { get; set; }
    public string PACode { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
    public bool Success { get; set; }
    public string Errors { get; set; }
    public string Comments { get; set; }
}