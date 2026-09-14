using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.InterfaceOptions;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.InterfaceOptions;

public class OemUserMappingResolver
    : IValueResolver<FusionVolvoInterfaceOptions, Volvo.Common.Settings.Models.InterfaceOptions, IDictionary<string, string>>
{
    public IDictionary<string, string> Resolve(FusionVolvoInterfaceOptions source, Volvo.Common.Settings.Models.InterfaceOptions destination, IDictionary<string, string> destMember, ResolutionContext context)
    {
        var oemUserMapping = new Dictionary<string, string>();

        foreach (var mapping in source.OEMUserCrossReferences ?? Array.Empty<OEMUserCrossReference>())
        {
            var strippedUsername = mapping.Username.Replace(".", string.Empty);
            oemUserMapping[strippedUsername] = mapping.OEMUserID;
        }

        return oemUserMapping;
    }

}
