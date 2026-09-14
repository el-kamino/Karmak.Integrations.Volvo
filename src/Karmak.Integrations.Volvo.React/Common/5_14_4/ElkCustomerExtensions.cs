using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Core.Common.V5_14_4
{
    public static class CustomerExtensions
    {
        private const string FUSION_ID = "FUSION";
        private const string VOLVO_PASS_REWARD_ID = "VolvoPASSRewardID";

        public static string GetFusionId(this Customer customer) =>
            customer.GetExternalIdentifierBySourceType(FUSION_ID);

        public static string GetVolvoPassRewardId(this Customer customer) =>
            customer.GetExternalIdentifierBySourceType(VOLVO_PASS_REWARD_ID);

        public static string SetVolvoPassRewardId(this Customer customer,string volvoPassRewardId) =>
            customer.SetExternalIdentifierBySourceType(VOLVO_PASS_REWARD_ID,volvoPassRewardId);

        private static string GetExternalIdentifierBySourceType(this Customer customer, string source) =>
            customer?.ExternalIdentifiers?.FirstOrDefault(id => id.ExternalSourceType.Equals(source))?.ID;

        private static string SetExternalIdentifierBySourceType(this Customer customer, string source, string id)
        {
            if (customer == null)
                return null;

            if (customer.ExternalIdentifiers == null)
                customer.ExternalIdentifiers = new List<ExternalIdentifier>();

            var existingId = GetExternalIdentifierBySourceType(customer, source);

            if (existingId == id) // it is already set correctly, just leave
                return existingId;

            if (!string.IsNullOrEmpty(existingId))
                customer.ExternalIdentifiers.Remove(new ExternalIdentifier() { ExternalSourceType = source, ID = existingId });

            customer.ExternalIdentifiers.Add(new ExternalIdentifier() { ExternalSourceType = source, ID = id });

            return existingId;
        }
    }
}
