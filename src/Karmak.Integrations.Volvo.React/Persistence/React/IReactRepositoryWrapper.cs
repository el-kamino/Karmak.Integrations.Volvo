using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public interface IReactRepositoryWrapper
    {
        Task CreateRepairOrderAsync(string kan, string paCode, RepairOrderSnapshot repairOrder);
        Task CreateCustomerUpdateAsync(string kan, string paCode, Contracts.CustomerUpdates.Data.CustomerUpdate repairOrder);
        Task CreatePartsSalesOrderAsync(string kan, string paCode, PartsSalesOrder partsSalesOrder);
        Task CreateVehicleSalesOrderAsync(string kan, string paCode, VehicleSalesOrder vehicleSalesOrder);

        Task<List<RepairOrderSnapshot>> QueryRepairOrdersAsync(string paCode, RepairOrderSnapshotCriteria criteria);
        Task<List<PartsSalesOrder>> QueryPartsSalesOrdersAsync(string paCode, PartsSalesOrderSearchCriteria criteria);
        Task<List<VehicleSalesOrder>> QueryVehicleSalesOrdersAsync(string paCode, VehicleSalesOrderSearchCriteria criteria);
        Task<List<Contracts.CustomerUpdates.Data.CustomerUpdate>> QueryCustomerUpdatesAsync(string paCode, CustomerUpdateSearchCriteria criteria);
    }
}
