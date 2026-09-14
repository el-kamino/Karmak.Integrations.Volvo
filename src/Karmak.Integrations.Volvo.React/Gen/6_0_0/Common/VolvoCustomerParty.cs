using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0
{
    public class VolvoCustomerParty
    {
        //commented propoerties are not being sent by volvo as part of the payload, so not adding to the model for now. If needed in future, can be added back.

        public string partyType { get; set; }
        public string customerType { get; set; }
        public string companyName { get; set; }
        //public string companyDescription { get; set; }
        public string givenName { get; set; }
        public string middleName { get; set; }
        public string familyName { get; set; }
        //public string title { get; set; }
        //public string salutation { get; set; }
        //public string genderCode { get; set; }
        //public string specifiedOccupationTitle { get; set; }
        public string languageCode { get; set; }
        //public string manufacturerCustomerId { get; set; }
        //public string manufacturerHouseholdId { get; set; }
        public List<VolvoCustomerNumber> customerNumber { get; set; }
        public List<VolvoTelephoneCommunication> telephoneCommunication { get; set; }
        public List<VolvoUriCommunication> uriCommunication { get; set; }
        //public List<VolvoMessagingAppCommunication> messagingAppCommunication { get; set; }
        public VolvoAddressWithPrivacy postalAddress { get; set; }
        //public List<VolvoPrivacy> privacy { get; set; }
    }

    public class VolvoCustomerNumber
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class VolvoTelephoneCommunication
    {
        public string channelCode { get; set; }
        public string completeNumber { get; set; }
        public List<VolvoPrivacy> privacy { get; set; }
    }

    public class VolvoUriCommunication
    {
        public string uriId { get; set; }
        public string channelCode { get; set; }
        public List<VolvoPrivacy> privacy { get; set; }
    }

    public class VolvoMessagingAppCommunication
    {
        public string uriid { get; set; }
        public string completeNumber { get; set; }
        public string serviceProviderName { get; set; }
        public List<VolvoPrivacy> privacy { get; set; }
    }
}
