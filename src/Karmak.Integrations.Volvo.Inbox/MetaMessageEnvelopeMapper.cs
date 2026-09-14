using Karmak.Integrations.Volvo.Inbox.Models;

namespace Karmak.Integrations.Volvo.Inbox;

public static class MetaMessageEnvelopeMapper
{
    public static MetaMessageEnvelope Map(FixedWidthFile file, (string, string) accountAndBranch,
           string contentUri)
    {
        var (accountNumber, branchId) = accountAndBranch;

        return new MetaMessageEnvelope
        {
            AccountNumber = accountNumber,
            BranchId = branchId,
            PACode = file.PACode,
            IsActive = true,
            SystemCreatedDate = DateTime.UtcNow,
            SystemLastModifiedDate = DateTime.UtcNow,
            MessageType = InboxMessageType.FixedWidthFile,
            FileName = file.FileName,
            FileType = file.FileType,
            SourceId = file.SourceId,
            Oem = ModelConstants.OEM_NAME_VOLVO,
            ContentUri = contentUri,
            CreatedDate = DateTime.SpecifyKind(file.CreatedDate, DateTimeKind.Utc),
            FileDescription = file.FileDescription
        };
    }
}