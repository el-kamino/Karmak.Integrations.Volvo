using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.React.ServiceAppointment;

internal static class ConfigurationExtensions
{
    public static IServiceCollection AddServiceAppointmentsDispatch(this IServiceCollection services)
    {
        services.AddKeyedScoped<IRequestDispatcher, ServiceAppointmentDispatcher>(ServiceAppointmentDispatcher.EntityType);
        return services;
    }
}
