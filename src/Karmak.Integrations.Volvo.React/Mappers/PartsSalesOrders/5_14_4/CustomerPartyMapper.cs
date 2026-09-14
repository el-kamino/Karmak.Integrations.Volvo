using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.PartsSalesOrders.V5_14_4
{
    public static class CustomerPartyMapper
    {
        public static PartyABIEType MapShippingCustomer(Customer customer, VolvoSettings settings)
        {
            return customer.BusinessStructure.EqualsIgnoreCase("Individual")
                ? new PartyABIEType
                {
                    Item = SpecifiedPersonMapper.MapShipToParty(customer, settings.RegionSettings.LanguageCode)
                }
                : new PartyABIEType
                {
                    Item = OrganizationMapper.MapShipToParty(customer)
                };
        }

        public static PartyABIEType MapBillingCustomer(Customer customer, VolvoSettings settings)
        {
            return customer.BusinessStructure.EqualsIgnoreCase("Individual")
                ? new PartyABIEType
                {
                    Item = SpecifiedPersonMapper.MapBillToParty(customer, settings.RegionSettings.LanguageCode)
                }
                : new PartyABIEType
                {
                    Item = OrganizationMapper.MapBillToParty(customer)
                };
        }

    }
}