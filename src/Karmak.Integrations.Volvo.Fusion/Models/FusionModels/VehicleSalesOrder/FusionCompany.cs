using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.VehicleSalesOrder
{
    public class FusionCompany
    {
        public string CompanyName { get; set; }
        public IList<FusionContact> Contacts { get; set; }
    }
}