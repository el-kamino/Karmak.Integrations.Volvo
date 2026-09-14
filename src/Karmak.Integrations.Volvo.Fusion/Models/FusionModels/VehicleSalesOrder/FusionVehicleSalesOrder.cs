using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.VehicleSalesOrder
{
    public class FusionVehicleSalesOrder
    {
        public string DatabaseVersion { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDateTime { get; set; }
        public string SalesPerson { get; set; }
        public FusionCustomer BillingCustomer { get; set; }
        public IList<FusionVehicle> VehiclesSold { get; set; }
        public IList<FusionVehicle> VehiclesTraded { get; set; }
    }
}