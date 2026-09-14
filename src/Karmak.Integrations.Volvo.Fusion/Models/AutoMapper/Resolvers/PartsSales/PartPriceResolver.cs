using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsSales
{
    public class PartPriceResolver : IValueResolver<FusionPart, Part, decimal> {
        private const decimal NO_CHARGE = 0.0m;

        public decimal Resolve(FusionPart source, Part destination, decimal destMember, ResolutionContext context) {
            return IsNoChargePart(source.ExtendedPrice)
                ? NO_CHARGE
                : source.UnitPrice;
        }

        private bool IsNoChargePart(decimal partCharge) {
            return partCharge == NO_CHARGE;
        }
    }
}
