using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public interface IVolvoEventEvaluator
    {
        bool PreventsFurtherEvaluations { get; }
        void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history);
    }
}