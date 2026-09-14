using Karmak.Integrations.Volvo.React.Contracts.Common;
using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data
{
    public class VehicleSalesOrder: IOverridable {
        public PayloadMetadata Metadata { get; set; }
        public bool ForceTransmission { get; set; }
        public DealerInfo DealerInfo { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? SalesDate { get; set; }
        public string SalesPersonId { get; set; }
        public Customer Customer { get; set; }
        public IList<Vehicle> SoldVehicles { get; set; }
        public IList<Vehicle> TradeInVehicles { get; set; }
        public decimal? TimeZone { get; set; }

        public VehicleSalesOrder() {
            SoldVehicles = new List<Vehicle>();
            TradeInVehicles = new List<Vehicle>();
        }
    }
}