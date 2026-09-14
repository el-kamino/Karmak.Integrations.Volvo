using Azure.Data.Tables;

namespace Karmak.Integrations.Volvo.Inbox.Models
{
    public interface IMetaDataEntity : ITableEntity
    {
        string Id { get; set; }
        bool IsActive { get; set; }
        DateTime SystemLastModifiedDate { get; set; }
    }
}