using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.Fusion.Models.CustomMappers.PartsSales;

public static class CustomerPhoneMapper
{
    public static IList<Phone> Map(FusionCustomer source)
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
