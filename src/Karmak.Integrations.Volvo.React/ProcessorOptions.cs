using Karmak.Integrations.Volvo.React.Contracts;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React;

internal class ProcessorOptions
{
    public enum SendStatus
    {
        None = -1,
        Production = 1,
        Pilot = 2
    }

    public string Environment { get; set;  }
    public EntityVersionOptions VersionOptions { get; set; } = new EntityVersionOptions();

    public int GetSendStatus(string entityType, string version, string branchIdentifier)
    {
        if (VersionOptions == null)  //not setup, send production by default
            return (int)SendStatus.Production;

        var entityVersionOptions = entityType switch
        {
            EntityTypes.RepairOrder => VersionOptions.RepairOrdersVersionOptions,
            EntityTypes.PartSalesOrder => VersionOptions.PartSalesVersionOptions,
            EntityTypes.VehicleSale => VersionOptions.VehicleSalesVersionOptions,
            EntityTypes.PartsInventory => VersionOptions.PartsInventoryVersionOptions,
            EntityTypes.CustomerUpdate => VersionOptions.CustomerUpdatesVersionOptions,
            _ => null
        };

        if (entityVersionOptions == null) //not setup, send production by default
            return (int)SendStatus.Production;
       
        return GetSendStatusByEntity(entityVersionOptions, version, branchIdentifier);
    }

    private int GetSendStatusByEntity(EntityVersionOption entityVersionOptions, string version, string BranchIdentifier)
    {
        //if no versions are set, assume we are in production=send, else if pilot see if branch matches.
        if ((entityVersionOptions.ProductionVersion == null && entityVersionOptions.PilotVersion == null) || version == entityVersionOptions.ProductionVersion)
        {
            return (int)SendStatus.Production;
        }
        else if (version == entityVersionOptions.PilotVersion && entityVersionOptions.PilotBranches.Contains(BranchIdentifier))
        {
            return (int)SendStatus.Pilot;
        }
        return (int)SendStatus.None;
    }
}

internal class EntityVersionOptions
{
    public EntityVersionOption RepairOrdersVersionOptions { get; set; } = new EntityVersionOption();
    public EntityVersionOption PartSalesVersionOptions { get; set; } = new EntityVersionOption();
    public EntityVersionOption VehicleSalesVersionOptions { get; set; } = new EntityVersionOption();
    public EntityVersionOption PartsInventoryVersionOptions { get; set; } = new EntityVersionOption();
    public EntityVersionOption CustomerUpdatesVersionOptions { get; set; } = new EntityVersionOption();
}

internal class EntityVersionOption
{
    public string ProductionVersion { get; set; }
    public string PilotVersion { get; set; }
    public IList<string> PilotBranches { get; set; }
}
