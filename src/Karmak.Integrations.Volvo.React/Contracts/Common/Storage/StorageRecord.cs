using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Karmak.Integrations.Volvo.React.Contracts.Common.Storage
{
    public abstract class StorageRecord<T> where T : class
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("_ts")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public T Snapshot { get; set; }
        [JsonIgnore]
        public T Body { get => Snapshot; set => Snapshot = value; }
    }
}
