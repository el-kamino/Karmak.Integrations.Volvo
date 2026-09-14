using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Storage
{
    public class ClaimJobCorrelationTableEntity : ITableEntity, IWarrantyTableEntity
    {
        public ClaimJobCorrelationTableEntity() { }
        public ClaimJobCorrelationTableEntity(string paCode, string correlationId)
        {
            RowKey = correlationId;
            PartitionKey = paCode;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        //claim.Id
        public string SerializedClaimJobIds { get; set; }
        [IgnoreDataMember]
        public IEnumerable<string> ClaimJobIds { get; set; }
        //claim.RepairOrder.SecondaryIdentifier
        public string RepairOrderNumber { get; set; }

        public void PopulateFollowingLoad()
        {
            ClaimJobIds = JsonConvert.DeserializeObject<IEnumerable<string>>(SerializedClaimJobIds);
        }

        public void PrepareToSave()
        {
            SerializedClaimJobIds = JsonConvert.SerializeObject(ClaimJobIds);
        }
    }
}
