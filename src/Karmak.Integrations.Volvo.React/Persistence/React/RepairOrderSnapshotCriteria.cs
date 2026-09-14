using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public class RepairOrderSnapshotCriteria
    {
        public DateTime? WindowStart { get; set; }
        public DateTime? WindowEnd { get; set; }
        public bool OnlyLastSnapshotPerRepairOrder { get; set; } = false;
        public IEnumerable<string> RepairOrderNumbers { get; set; }

        public RepairOrderSnapshotCriteria()
        {
            RepairOrderNumbers = new List<string>();
        }

        public bool IsProperlyConstrained => IsDateConstrained || IsRepairOrderConstrained;
        public bool IsDateConstrained => (WindowStart.HasValue && WindowEnd.HasValue);
        public bool IsRepairOrderConstrained => RepairOrderNumbers.Any();
    }
}
