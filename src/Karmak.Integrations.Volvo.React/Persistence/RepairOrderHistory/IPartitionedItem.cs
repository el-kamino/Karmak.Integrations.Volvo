namespace Karmak.Integrations.Volvo.React.Persistence.RepairOrderHistory
{
    public interface IPartitionedItem
    {
        string Id { get; }
        string PartitionKey { get; }
        string Etag { get; }
    }
}
