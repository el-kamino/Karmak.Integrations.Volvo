using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Persistence.RepairOrderHistory;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class VolvoEventHistory : IPartitionedItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty]
        public string PartitionKey { get; set; }
        [JsonProperty]
        public string RepairOrderNumber { get; set; }
        [JsonProperty]
        public int LastKnownSnapshotSequenceNumber { get; set; }
        [JsonProperty]
        public List<VolvoEvent> Events { get; set; }
        [JsonIgnore]
        public List<VolvoEvent> PendingEvents { get; set; }
        [JsonProperty("_etag")]
        public string Etag { get; set; }
        [JsonProperty]
        public string LastKnownSnapshotCommentsHash { get; set; }

        public IEnumerable<VolvoEvent> GetOutboxEvents(Guid roSnapshotId) =>
            Events.Where(e => !e.IsTransmitted && e.RelatedSnapshotId == roSnapshotId);

        public VolvoEventHistory(string id = null)
        {
            Id = id;
            Events = new List<VolvoEvent>();
            PendingEvents = new List<VolvoEvent>();
            LastKnownSnapshotCommentsHash = null;
        }

        public void MarkEventAsTransmitted(VolvoEvent volvoEvent)
        {
            foreach (var evt in Events)
            {
                if (evt.Id == volvoEvent.Id)
                    evt.TransmittedTime = DateTime.Now;
            }
        }

        public void ApplySnapshotIdToAllPendingEvents(Guid roSnapshotId)
        {
            foreach (var volvoEvent in PendingEvents)
            {
                volvoEvent.RelatedSnapshotId = roSnapshotId;
            }
        }
        public void ApplyAllPendingEventsToJournalHistory()
        {
            foreach (var volvoEvent in PendingEvents)
            {
                WithNewVolvoEvent(volvoEvent);
            }
        }

        public bool HasPendingEvents()
        {
            return PendingEvents.Count > 0;
        }

        public bool HasNoPendingEvents()
        {
            return !HasPendingEvents();
        }

        public void AddPending(VolvoEvent volvoEvent)
        {
            if (!volvoEvent.Matches(AtDealership.DESCRIPTION) && DoesNotContain(AtDealership.DESCRIPTION))
            {
                PendingEvents.Add(new AtDealership
                {
                    Timestamp = volvoEvent.Timestamp
                });
            }
            PendingEvents.Add(volvoEvent);
        }

        public VolvoEventHistory WithNewVolvoEvent(VolvoEvent volvoEvent)
        {
            Events.Add(volvoEvent);
            return this;
        }

        public bool HasHistory()
        {
            return Events.Count > 0;
        }

        public bool HasNoHistory()
        {
            return !HasHistory();
        }

        public bool Contains(string volvoEventDescription)
        {
            return Events.Any(s => s.Matches(volvoEventDescription)) || PendingEvents.Any(s => s.Matches(volvoEventDescription));
        }

        public bool Contains(VolvoEvent volvoEvent)
        {
            return Contains(volvoEvent.Description);
        }

        public bool DoesNotContain(string volvoEventDescription)
        {
            return !Contains(volvoEventDescription);
        }

        public bool DoesNotContain(VolvoEvent volvoEvent)
        {
            return !Contains(volvoEvent.Description);
        }
    }
}