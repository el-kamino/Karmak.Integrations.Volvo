using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4
{
    internal interface IProcessPartsSalesOrder
    {
        Task Execute(PartsSalesOrder partsSalesOrder, string newOrHistorical);
    }
}