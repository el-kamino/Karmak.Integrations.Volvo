using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsInventory;

public class ReportTypeResolver : IValueResolver<FusionPartsInventoryReport, PartsInventoryReport, ReportType>
{
    private const string FULL = "FULL";
    public ReportType Resolve(FusionPartsInventoryReport source, PartsInventoryReport destination, ReportType destMember, ResolutionContext context)
    {
        return source.PartInventoryMetaData.RecordSetType.Equals(FULL)
            ? ReportType.Full
            : ReportType.Delta;
    }
}
