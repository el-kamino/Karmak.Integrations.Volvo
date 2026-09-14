using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0;
using Karmak.Integrations.Volvo.React.Utils;
using MassTransit.Transports;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Mappers.Shared._6_0_0
{
    internal static class VolvoCustomerPartyBuilder
    {
        public static VolvoCustomerParty BuildOwnerParty(Customer customer, Address address, string languageCode)
        {
            var contact = customer.Contacts?.FirstOrDefault();

            return new VolvoCustomerParty
            {
                partyType = CustomerTypeCodes.PartyTypeOwner,
                customerType = GetCustomerType(customer),
                companyName = string.IsNullOrWhiteSpace(customer?.CompanyName) ? null : customer.CompanyName.WafSanitize().MaxLength(80),
                givenName = string.IsNullOrWhiteSpace(contact?.FirstName) ? null : contact.FirstName.WafSanitize().MaxLength(30),
                middleName = string.IsNullOrWhiteSpace(contact?.MiddleName) ? null : contact.MiddleName.WafSanitize().MaxLength(1),
                familyName = string.IsNullOrWhiteSpace(contact?.LastName) ? null : contact.LastName.WafSanitize().MaxLength(80),
                languageCode = languageCode,
                customerNumber = BuildCustomerNumbers(customer?.CustomerKey, CustomerTypeCodes.IdTypeDms, customer?.ExternalIdentifiers),
                telephoneCommunication = BuildOwnerPartyPhones(contact),
                uriCommunication = BuildUris(contact?.Email),
                postalAddress = VolvoAddressBuilder.BuildAddress(address),
            };
        }

        public static VolvoCustomerParty BuildDriverParty(Contact driver, Address address, string languageCode)
        {
            if (driver == null)
                return null;

            return new VolvoCustomerParty
            {
                partyType = CustomerTypeCodes.PartyTypeDriver,
                customerType = CustomerTypeCodes.CustomerTypeIndividual,
                givenName = driver?.FirstName.WafSanitize().MaxLength(30) ?? "",
                middleName = driver?.MiddleName.WafSanitize().MaxLength(1) ?? "",
                familyName = driver?.LastName.WafSanitize().MaxLength(80) ?? "",
                languageCode = languageCode,
                customerNumber = BuildCustomerNumbers(driver.LicenseNumber, CustomerTypeCodes.IdTypeGovernment, []),
                telephoneCommunication = BuildDriverPhones(driver),
                uriCommunication = BuildUris(driver.Email),
                postalAddress = VolvoAddressBuilder.BuildAddress(address),
            };
        }

        private static List<VolvoCustomerNumber> BuildCustomerNumbers(string identifier, string type, IList<ExternalIdentifier> externalIdentifiers)
        {
            var customerNumbers = new List<VolvoCustomerNumber>();
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                customerNumbers.Add(new VolvoCustomerNumber
                {
                    id = identifier.MaxLength(30),
                    type = type
                });
            }
            if (externalIdentifiers != null)
            {
                foreach (var externalIdentifier in externalIdentifiers)
                {
                    if (externalIdentifier.ExternalSourceType == "FUSION")
                    {
                        customerNumbers.Add(new VolvoCustomerNumber
                        {
                            id = externalIdentifier.ID.MaxLength(30),
                            type = CustomerTypeCodes.IdTypeSystem
                        });
                    }
                }
            }
            return customerNumbers;
        }

        private static List<VolvoTelephoneCommunication> BuildOwnerPartyPhones(Contact contact)
        {
            var workPhone = BuildTelephone(contact, PhoneType.WORK);
            var homePhone = BuildTelephone(contact, PhoneType.HOME);
            var cellPhone = BuildTelephone(contact, PhoneType.CELL);

            return new[] { workPhone, homePhone, cellPhone }
                .Where(phone => phone != null)
                .ToList();
        }

        private static List<VolvoTelephoneCommunication> BuildDriverPhones(Contact driver)
        {
            var workPhone = BuildTelephone(driver, PhoneType.WORK);
            
            if (workPhone != null)
            {
                // per Volvo, use CELL channel code for driver phone stored in work phone
                workPhone.channelCode = PhoneType.CELL.ToString().ToLower();
                return new List<VolvoTelephoneCommunication> { workPhone };
            }
                
            return new List<VolvoTelephoneCommunication>();
        }

        private static VolvoTelephoneCommunication BuildTelephone(Contact contact, PhoneType type)
        {
            var phone = contact?.Phones.FirstOrDefault(phone => phone.Type == type);
            if (phone != null && !string.IsNullOrWhiteSpace(phone.Number))
            {
                return new VolvoTelephoneCommunication()
                {
                    channelCode = phone.Type.ToString().ToLower(),
                    completeNumber = AlphanumericPhoneTranslator.ToNumeric(phone.Number).MaxLength(20)
                };
            }
            return null; ;
        }

        private static List<VolvoUriCommunication> BuildUris(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new List<VolvoUriCommunication>();

            return new List<VolvoUriCommunication>()
            {
                new VolvoUriCommunication
                {
                    uriId = email.MaxLength(100),
                    channelCode = CustomerTypeCodes.EmailChannelCode
                }
            };
        }

        private static string GetCustomerType(Customer customer)
        {
            if (customer.BusinessStructure != null && customer.BusinessStructure == "Individual")
            {
                return CustomerTypeCodes.CustomerTypeIndividual;
            }
            return CustomerTypeCodes.CustomerTypeCompany;
        }
    }
}
