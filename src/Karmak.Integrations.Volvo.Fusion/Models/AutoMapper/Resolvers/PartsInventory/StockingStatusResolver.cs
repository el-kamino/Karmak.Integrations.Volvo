using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsInventory;

public class StockingStatusResolver : IValueResolver<FusionPart, InventoryPart, StockingStatus>
{
    public const string STOCK = "Stock";
    public const string NON_STOCK = "Non-Stock";
    public StockingStatus Resolve(FusionPart source, InventoryPart destination, StockingStatus destMember, ResolutionContext context)
    {
        switch (source.StockStatus)
        {
            case STOCK:
                return StockingStatus.Stocked;
            case NON_STOCK:
                return StockingStatus.NonStocked;
            default:
                return StockingStatus.Unknown;
        }
    }
}
