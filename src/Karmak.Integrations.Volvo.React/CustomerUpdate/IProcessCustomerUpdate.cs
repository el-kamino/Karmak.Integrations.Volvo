using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.CustomerUpdate
{
    public interface IProcessCustomerUpdate
    {
        Task Execute(Contracts.CustomerUpdates.Data.CustomerUpdate customerInfo, string newOrHistorical);
    }
}