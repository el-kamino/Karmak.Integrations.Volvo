using Karmak.Integrations.Volvo.React.Comments;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Persistence.RepairOrderHistory;
using Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.React.Configuration;

public static partial class ConfigurationExtensions
{
    private static IServiceCollection AddRepairOrdersProcessing(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<EmailSenderOptions>(options =>
        {
            options.AlertUri = config["Volvo:React:RepairOrders:Comments:EmailAlertUri"];
        });

        services.Configure<CommentsServiceOptions>(options =>
        {
            options.CommentsBaseUrl = config["Volvo:React:RepairOrders:Comments:BaseUri"];
            options.CommentsReceivePath = config["Volvo:React:RepairOrders:Comments:ReceivePath"];

            options.ClientId = config["Volvo:React:OAuth:ClientId"];
            options.ClientSecret = config["Volvo:React:OAuth:ClientSecret"];
            options.Resource = config["Volvo:React:OAuth:Resource"];
            options.TokenUri = config["Volvo:React:OAuth:TokenUri"];
        });

        services.AddHttpClient<IEmailSender, EmailSender>();
        services.AddHttpClient<ICommentsService, CommentsService>();

        services.AddSingleton<IDocumentStore<VolvoEventHistory>>(sp =>
        {
            var cc = sp.GetRequiredService<CosmosClient>();
            var options = Options.Create(new DocumentStoreOptions
            {
                CollectionId = config["Volvo:React:RepairOrders:DatabaseRepairOrderHistoryCollection"],
                DatabaseId = config["Volvo:React:Database:Id"]
            });

            return new DocumentStore<VolvoEventHistory>(cc, options);
        });

        services.AddScoped<IVolvoEventHistoryProvider, VolvoEventHistoryStore>();
        services.AddScoped<IVolvoRepairOrderStatusEvaluater, VolvoRepairOrderStatusEvaluator>();
        return services;
    }
}
