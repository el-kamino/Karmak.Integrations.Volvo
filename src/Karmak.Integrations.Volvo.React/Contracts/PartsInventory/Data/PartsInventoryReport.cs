using Karmak.Integrations.Volvo.React.Contracts.Common;
using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data
{
    public class PartsInventoryReport {
        public PayloadMetadata Metadata { get; set; }
        public DealerInfo DealerInfo { get; set; }
        public Guid Id { get; set; }
        public ReportType Type { get; set; }
        public IList<InventoryPart> Parts { get; set; }
        public decimal? TimeZone { get; set; }

        public PartsInventoryReport() {
            Id = Guid.NewGuid();
            Parts = new List<InventoryPart>();
        }
    }
}