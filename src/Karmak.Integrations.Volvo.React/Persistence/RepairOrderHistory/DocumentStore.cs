using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Persistence.RepairOrderHistory;

internal class DocumentStore<T> : IDocumentStore<T> where T : class, IPartitionedItem
{
    private readonly CosmosClient _client;
    private readonly string _collectionId;
    private readonly string _databaseId;

    public DocumentStore(CosmosClient client, IOptions<DocumentStoreOptions> options)
    {
        _collectionId = options.Value.CollectionId;
        _databaseId = options.Value.DatabaseId;
        _client = client;
    }

    /// <summary>
    /// Update or insert an item
    /// </summary>
    /// <param name="item">Object to insert or update</param>
    /// <returns>THE EXACT SAME THING AS WE PUT IN</returns>
    public async Task<T> UpsertItemAsync(T item)
    {
        var reqOpts = new ItemRequestOptions
        {
            IfMatchEtag = item.Etag
        };

        var doc = await _client
            .GetDatabase(_databaseId)
            .GetContainer(_collectionId)
            .UpsertItemAsync(item, new PartitionKey(item.PartitionKey), reqOpts);

        if (doc == null || doc.Resource == null)
        {
            return null;
        }

        //don't like this but not a clean way to go from Document -> Type unless Type extends Document... 
        // turns out this is stupid anyway, what we are returning is exactly what we put in.  Having
        // the return value isn't for getting an update, it is for call chaining.  
        var serializedDoc = JsonConvert.SerializeObject(doc.Resource);
        return JsonConvert.DeserializeObject<T>(serializedDoc);
    }

    public async Task<T> GetItemAsync(string id, string partitionKey)
    {
        try
        {
            return await _client
                .GetDatabase(_databaseId)
                .GetContainer(_collectionId)
                .ReadItemAsync<T>(id, new PartitionKey(partitionKey));
        }
        catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}