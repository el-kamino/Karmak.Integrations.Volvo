namespace Karmak.Integrations.Volvo.Fusion.React.RepairOrders.MessageOrdering;

public interface IAtomicCounter
{
    Task<int> IncrementAsync(string partitionKey, string counterName, CancellationToken cancellationToken = default);
}
