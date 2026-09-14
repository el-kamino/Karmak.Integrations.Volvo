using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using ElkCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;

public class CustomerPhonesResolver : IValueResolver<FusionCustomer, ElkCustomer, IList<Phone>>
{
    public IList<Phone> Resolve(FusionCustomer source, ElkCustomer destination, IList<Phone> destMember, ResolutionContext context)
    {
        var phoneMappings = new List<(PhoneType type, string number)>
        {
            (PhoneType.WORK, source.OfficePhone),
            (PhoneType.CELL, source.CellPhone),
        };

        return phoneMappings
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.number))
            .Select(mapping => new Phone
            {
                Type = mapping.type,
                Number = mapping.number
            }).ToList();
    }
}