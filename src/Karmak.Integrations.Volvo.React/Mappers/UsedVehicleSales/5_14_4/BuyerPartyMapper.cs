using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Core.Common.V5_14_4;
using System.Linq;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.UsedVehicleSales.V5_14_4
{
    public static class BuyerPartyMapper
    {
        public static PartyABIEType[] Map(Customer customer, VolvoSettings settings)
        {
            if (customer is null)
                return null;

            var party = customer.BusinessStructure == "Individual"
                ? FetchIndividualPartyOrNull(customer.Contacts?.FirstOrDefault(), customer, settings.RegionSettings.LanguageCode)
                : FetchOrganizationPartyOrNull(customer);

            var volvoPassRewardId = customer.GetVolvoPassRewardId();
            if (!string.IsNullOrEmpty(volvoPassRewardId))
                party.ManufacturerHouseholdID = new IdentifierType
                {
                    Value = volvoPassRewardId
                };

            return new[] { party };
        }

        private static PartyABIEType FetchIndividualPartyOrNull(Contact contact, Customer customer, string languageCode)
        {
            // from Motive spec: individual requires last name to be valid
            if (string.IsNullOrWhiteSpace(contact?.LastName))
                return null;

            return new PartyABIEType
            {
                Item = ContactMapper.Map(contact, customer, languageCode)
            };
        }

        private static PartyABIEType FetchOrganizationPartyOrNull(Customer customer)
        {
            // from Motive spec: organization only requires company name
            if (string.IsNullOrWhiteSpace(customer.CompanyName))
                return null;

            return new PartyABIEType
            {
                Item = OrganizationMapper.Map(customer)
            };
        }
    }
}