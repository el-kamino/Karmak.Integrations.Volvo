using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile;
using Karmak.Integrations.Volvo.Fusion.React.CustomerUpdate;
using Karmak.Integrations.Volvo.Fusion.React.PartsInventory;
using Karmak.Integrations.Volvo.Fusion.React.PartsSalesOrders;
using Karmak.Integrations.Volvo.Fusion.React.RepairOrders;
using Karmak.Integrations.Volvo.Fusion.React.ServiceAppointment;
using Karmak.Integrations.Volvo.Fusion.React.UsedVehicleSales;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.React;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddVolvoReactDispatch(this IServiceCollection services, IConfiguration config)
    {
        services.AddAutoMapper(typeof(RepairOrderMappingProfile));
        services.AddAutoMapper(typeof(PartsSalesOrderMappingProfile));
        services.AddAutoMapper(typeof(VehicleSalesOrderMappingProfile));
        services.AddAutoMapper(typeof(PartsInventoryReportMappingProfile));
        services.AddAutoMapper(typeof(CustomerUpdateMappingProfile));

        services.AddSingleton<IDealerInfoExtractor, DealerInfoExtractor>();

        services.AddRepairOrderDispatch(config);
        services.AddPartsSalesOrderDispatch(config);
        services.AddUsedVehicleSalesDispatch(config);
        services.AddPartsInventoryDispatch();
        services.AddCustomerUpdateDispatch(config);
        services.AddServiceAppointmentsDispatch();

        return services;
    }
}
