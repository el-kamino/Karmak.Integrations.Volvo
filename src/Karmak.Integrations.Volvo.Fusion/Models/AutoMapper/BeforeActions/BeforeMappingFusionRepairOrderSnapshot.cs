using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.BeforeActions;

public class BeforeMappingFusionRepairOrderSnapshot : IMappingAction<FusionRepairOrderSnapshot, RepairOrderSnapshot>
{
    public void Process(FusionRepairOrderSnapshot source, RepairOrderSnapshot destination, ResolutionContext context)
    {
        if (ShouldNotMap(source.Driver))
        {
            source.Driver = null;
        }
        if (ShouldNotMap(source.Appointment))
        {
            source.Appointment = null;
        }
    }

    private static bool ShouldNotMap(FusionContact contact)
    {
        return contact == null ||
           typeof(FusionContact)
            .GetProperties()
            .Where(prop => prop.PropertyType == typeof(string))
            .Select(prop => prop.GetValue(contact, null))
            .All(val => val == null);
    }

    private static bool ShouldNotMap(FusionAppointment appointment)
    {
        return appointment == null ||
            typeof(FusionAppointment)
            .GetProperties()
            .All(prop => prop.GetValue(appointment) == null);
    }
}