using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsInventory;

public class QuantitySoldHistoryResolver : IValueResolver<FusionPart, InventoryPart, IList<QuantitySoldRecord>>
{
    private const int MONTHS_PER_YEAR = 12;

    IList<QuantitySoldRecord> IValueResolver<FusionPart, InventoryPart, IList<QuantitySoldRecord>>.Resolve(FusionPart source, InventoryPart destination, IList<QuantitySoldRecord> destMember, ResolutionContext context)
    {
        return source.SalesHistory.Select(record => new QuantitySoldRecord
        {
            Period = GetNumberOfMonthsAgo(record.Period).ToString(),
            QuantitySold = record.QuantitySold
        }).ToArray();
    }

    private int GetNumberOfMonthsAgo(DateTime period)
    {
        return (DateTime.Now.Month + DateTime.Now.Year * MONTHS_PER_YEAR) - (period.Month + period.Year * MONTHS_PER_YEAR);
    }
}
