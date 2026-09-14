using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsInventory;

public class DataForCurrentPeriodResolver : IValueResolver<FusionPart, InventoryPart, CurrentPeriodData>
{
    public CurrentPeriodData Resolve(FusionPart source, InventoryPart destination, CurrentPeriodData destMember, ResolutionContext context)
    {
        return new CurrentPeriodData
        {
            QuantityOnHand = source.QuantityOnHand,
            QuantitySold = source.SoldToday,
            QuantityAdjustment = source.AdjustmentToday,
            QuantityReceived = source.ReceivedToday,
            AdjustmentDescription = QuantityAdjustmentDescriptionType.Other
        };
    }
}
