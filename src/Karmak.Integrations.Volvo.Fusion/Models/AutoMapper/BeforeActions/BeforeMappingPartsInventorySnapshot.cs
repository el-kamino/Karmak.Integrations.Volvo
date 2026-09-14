using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.BeforeActions;

public class BeforeMappingPartsInventorySnapshot : IMappingAction<FusionPartsInventoryReport, PartsInventoryReport>
{
    public void Process(FusionPartsInventoryReport source, PartsInventoryReport destination, ResolutionContext context)
    {
        foreach (var part in source.Parts)
        {
            if (part.LastSold == default(DateTime))
            {
                part.LastSold = null;
            }
        }
    }
}
