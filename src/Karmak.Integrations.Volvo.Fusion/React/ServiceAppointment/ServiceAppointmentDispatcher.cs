using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Fusion.React.ServiceAppointment;

internal class ServiceAppointmentDispatcher : IRequestDispatcher
{
    public const string EntityType = "Service Appointment";

    public Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
    {
        //Service Appointments are no longer send to Volvo. However if a fusion instance submits one
        //we don't want to fail.
        return Task.CompletedTask;
    }
}
