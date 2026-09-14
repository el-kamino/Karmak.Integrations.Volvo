using System;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class CustomerContacted : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "CUSTOMER_CONTACTED";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations => false;

        public CustomerContacted() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (ro.CustomerContactedStatus.EqualsIgnoreCase("Ready for Pick-up") && history.DoesNotContain(this))
            {
                OccurredAt(ro.SnapshotSequenceNumberDateTime);
                history.AddPending(this);
            }
        }
    }
}