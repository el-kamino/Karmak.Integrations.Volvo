using Azure;
using Azure.Data.Tables;

namespace Karmak.Integrations.Volvo.Fusion.React.RepairOrders.MessageOrdering;

public class CounterEntity : ITableEntity
{
    public CounterEntity() 
    {
    }

    public CounterEntity(string partitionKey, string counterName)
    {
        PartitionKey = partitionKey ?? throw new ArgumentNullException(nameof(partitionKey));
        RowKey = counterName ?? throw new ArgumentNullException(nameof(counterName));
    }

    public int Value { get; set; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
