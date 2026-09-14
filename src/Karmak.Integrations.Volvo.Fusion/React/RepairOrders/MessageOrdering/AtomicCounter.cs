using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Fusion.React.RepairOrders.MessageOrdering;

internal class AtomicCounter : IAtomicCounter
{
    private readonly TableServiceClient _tableClient;
    private readonly string _countersTableName;

    public AtomicCounter(IOptions<AtomicCounterOptions> options)
    {
        _tableClient = new TableServiceClient(options.Value.TableConnectionString);
        _countersTableName = options?.Value?.TableName ?? throw new ArgumentNullException(nameof(options.Value.TableName));
    }

    public async Task<int> IncrementAsync(string partitionKey, string counterName, CancellationToken cancellationToken = default)
    {
        using var cancelationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(4));

        if (cancellationToken == default)
        {
            cancellationToken = cancelationTokenSource.Token;
        }
        else
        {
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancelationTokenSource.Token, cancellationToken).Token;
        }

        TableClient tc = _tableClient.GetTableClient(_countersTableName);

        while (true)
        {
            CounterEntity getResult = null;

            try
            {
                getResult = await tc.GetEntityAsync<CounterEntity>(partitionKey, counterName);
            }
            catch (RequestFailedException rfeGet) when (rfeGet.ErrorCode == "ResourceNotFound")
            {
                var counter = new CounterEntity(partitionKey, counterName)
                {
                    Value = 1
                };

                try
                {
                    await tc.AddEntityAsync(counter);
                    return 1;
                }
                catch (RequestFailedException rfeCreate) when (rfeCreate.ErrorCode == "EntityAlreadyExists")
                {
                    getResult = await tc.GetEntityAsync<CounterEntity>(partitionKey, counterName);
                }
            }

            getResult.Value += 1;

            try
            {
                await tc.UpdateEntityAsync(getResult, getResult.ETag, TableUpdateMode.Replace);
                return getResult.Value;
            }
            catch (RequestFailedException rfeUpdate) when (rfeUpdate.ErrorCode == "UpdateConditionNotSatisfied")
            {
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
