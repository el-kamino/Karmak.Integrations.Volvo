using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.VehicleSales
{
    public class CustomerPhoneResolver : IValueResolver<FusionCustomer, Contact, IList<Phone>>
    {
        public IList<Phone> Resolve(FusionCustomer source, Contact destination, IList<Phone> destMember, ResolutionContext context)
        {
            var phoneMappings = new List<(PhoneType type, string number)>
            {
                (PhoneType.CELL, source.CellPhone),
                (PhoneType.WORK, source.OfficePhone)
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
}