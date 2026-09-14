using Azure;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Inbox.Models
{
    public class MetaMessageEnvelope : IMetaDataEntity
    {
        [JsonIgnore] 
        public const string? MessageTypePrefix = "mt:";

        [JsonIgnore]
        public const string? SystemCreatedDatePrefix = "cd:";

        [JsonIgnore]
        public const string? BranchPrefix = "b:";

        [JsonIgnore]
        public const string? OemPrefix = "o:";

        public InboxMessageType MessageType
        {
            get => (InboxMessageType) MessageTypeId;
            set => MessageTypeId = (int) value;
        }

        public string? ContentUri { get; set; }

        public DateTime SystemLastModifiedDate { get; set; }

        public bool IsActive { get; set; }

        public string Id
        {
            get => RowKey;
            set => RowKey = value;
        }

        public string? AccountNumber
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public string? BranchId { get; set; }

        public string? PACode { get; set; }

        public int MessageTypeId { get; set; }

        public DateTime SystemCreatedDate { get; set; }

        public bool IsRead { get; set; }

        public bool IsPrinted { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? FileName { get; set; }

        public string? FileType { get; set; }

        public bool? Success { get; set; }

        public string? Errors { get; set; }

        public string? Comments { get; set; }

        public string? SourceId { get; set; }

        public string? CorrelationId { get; set; }

        public string? Oem { get; set; }
        
        public string? FileDescription { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public InboxMessage ToInboxMessage()
        {
            return new InboxMessage
            {
                Id = Id,
                BranchId = BranchId,
                PACode = PACode,
                MessageType = MessageType,
                IsActive = IsActive,
                IsPrinted = IsPrinted,
                IsRead = IsRead,
                Comments = Comments,
                CreatedDate = CreatedDate,
                Errors = Errors,
                FileName = FileName,
                FileType = FileType,
                SourceId = SourceId,
                Success = Success,
                Oem = Oem,
                FileDescription = FileDescription
            };
        }
    }
}