using Karmak.Integrations.Volvo.Fusion.React.RepairOrders.MessageOrdering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Fusion.React.RepairOrders
{
    internal static class ConfigurationExtensions
    {
        public static IServiceCollection AddRepairOrderDispatch(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<RepairOrderRetransmitDispatcherOptions>(options =>
            {
                options.QueueName = config["Volvo:React:RepairOrders:RetransmitQueueName"];
            });

            services.Configure<AtomicCounterOptions>(options =>
            {
                options.TableConnectionString = config["Volvo:Storage:ConnectionString"];
                options.TableName = config["Volvo:React:RepairOrders:CounterTableName"];
            });

            services.AddSingleton<IAtomicCounter, AtomicCounter>();
            services.AddKeyedScoped<IRequestDispatcher, RepairOrderSnapshotDispatcher>(RepairOrderSnapshotDispatcher.EntityType);
            services.AddScoped<IRepairOrderRetransmitDispatcher, RepairOrderRetransmitDispatcher>();

            return services;
        }
    }
}
