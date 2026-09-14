using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using ElkCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared
{
    public class CustomerTypeCodeResolver : IValueResolver<FusionCustomer, ElkCustomer, string> {
        public string Resolve(FusionCustomer source, ElkCustomer destination, string destMember, ResolutionContext context) {
            var formattedCustomerTypeCode = source.CustomerTypeCode?.ToUpper();

            return IsValidCustomerTypeCode(formattedCustomerTypeCode) ? formattedCustomerTypeCode : Constants.DefaultCustomerTypeCode;
        }

        private bool IsValidCustomerTypeCode(string customerTypeCode) {
            return !string.IsNullOrEmpty(customerTypeCode) && Constants.VolvoCustomerTypeCodes.Contains(customerTypeCode);
        }
    }
}
