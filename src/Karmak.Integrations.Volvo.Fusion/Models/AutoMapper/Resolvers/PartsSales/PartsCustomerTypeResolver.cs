using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsSales
{
    public class PartsCustomerTypeResolver : IValueResolver<FusionPartsSalesOrder, PartsSalesOrder, string> {
        private const string INTERNAL = "I";
        private const string RETAIL = "R";

        public string Resolve(FusionPartsSalesOrder source, PartsSalesOrder destination, string destMember, ResolutionContext context) {
            if (source.BillingCustomer == null) {
                return string.Empty;
            }
            return IsInternalSale(source.BillingCustomer)
                ? INTERNAL
                : RETAIL;
        }

        private bool IsInternalSale(FusionCustomer fusionCustomer) {
            return fusionCustomer.IsInternalLRAccount.Equals(true) || fusionCustomer.IsInternalSalesAccount.Equals(true);
        }
    }
}
