using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data
{
    public class InventoryPart {
        public string PartNumber { get; set; }
        public DateTime? LastSoldDate { get; set; }
        public StockingStatus StockingStatus { get; set; }
        public decimal QuantityBestStockingLevel { get; set; }
        public decimal ExtendedDealerCost { get; set; }
        public string BinLocation { get; set; }
        public CurrentPeriodData DataForCurrentPeriod { get; set; }
        public IList<QuantitySoldRecord> QuantitySoldHistory { get; set; }

        public bool IsVolvoPart { get; set; }
        public bool IsRimManaged { get; set; }
        public string TriggerReasonCode { get; set; }

        public InventoryPart() {
            QuantitySoldHistory = new List<QuantitySoldRecord>();
        }
    }
}