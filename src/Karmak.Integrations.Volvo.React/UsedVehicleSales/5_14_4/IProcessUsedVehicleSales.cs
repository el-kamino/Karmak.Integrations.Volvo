using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.UsedVehicleSales.V5_14_4
{
    public interface IProcessUsedVehicleSales
    {
        Task Execute(VehicleSalesOrder vehicleSalesOrder, string newOrHistorical);
    }
}