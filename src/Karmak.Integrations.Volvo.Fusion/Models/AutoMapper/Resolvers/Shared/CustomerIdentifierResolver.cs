using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using ElkCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared
{
    public class CustomerIdentifierResolver : IValueResolver<FusionCustomer, ElkCustomer, IList<ExternalIdentifier>> {
        public IList<ExternalIdentifier> Resolve(FusionCustomer source, ElkCustomer destination, IList<ExternalIdentifier> destMember, ResolutionContext context) {
            var externalIdentifiers = new List<ExternalIdentifier>();
            externalIdentifiers.Add(new ExternalIdentifier {
                ID = source.CustomerID,
                ExternalSourceType = "FUSION"
            });

            if (source.ExternalIdentifiers != null) {
                foreach (FusionExternalIdentifier id in source.ExternalIdentifiers) {
                    externalIdentifiers.Add(new ExternalIdentifier {
                        ID = id.ID,
                        ExternalSourceType = id.ExternalSourceType
                    });
                }
            }
            return externalIdentifiers;
        }
    }
}
