using System;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class VolvoEvent
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public virtual DateTime? TransmittedTime { get; set; } = null;
        public Guid RelatedSnapshotId { get; set; }

        [JsonIgnore]
        public bool IsTransmitted => TransmittedTime != null && TransmittedTime.Value > DateTime.MinValue;

        [JsonIgnore]
        public virtual bool TransmitToVolvo { get => true; }

        public VolvoEvent()
            : this(string.Empty)
        {
        }

        public VolvoEvent(string description)
            : this(description, description)
        {
        }

        public VolvoEvent(string description, string status)
        {
            Description = description;
            Status = status;
            Id = Guid.NewGuid();
        }

        public VolvoEvent OccurredAt(DateTime timestamp)
        {
            Timestamp = timestamp;
            return this;
        }

        public bool Matches(string description)
        {
            return Description == description;
        }

        public bool Matches(VolvoEvent other)
        {
            return Description == other.Description;
        }

        public bool IsExactInstance(VolvoEvent other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            return Id == other.Id;
        }

        protected bool RepairOrderIsInvoicedAccordingToTheLog(VolvoEventHistory history)
        {
            var lastInvoicedIndex = history.Events.FindLastIndex(state => state.Matches(Invoiced.DESCRIPTION));
            var lastReOpenedIndex = history.Events.FindLastIndex(state => state.Matches(ReOpened.DESCRIPTION));
            return lastInvoicedIndex > lastReOpenedIndex;
        }
    }

    public class VolvoEventWithoutTransmission : VolvoEvent
    {
        [JsonIgnore]
        public override bool TransmitToVolvo { get => false; }
        public override DateTime? TransmittedTime { get => null; set { } }
        public VolvoEventWithoutTransmission(string description) : base(description) { }
    }
}
