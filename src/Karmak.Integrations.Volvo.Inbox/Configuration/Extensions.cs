using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Storage.Table;
using Karmak.Integrations.Volvo.Inbox.Validators.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karmak.Integrations.Volvo.Inbox.Configuration
{
    public static class Extensions
    {
        public static IServiceCollection AddInboxServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IInboxRepository, InboxRepository>();
            services.AddSingleton<IMetaDataStore<MetaMessageEnvelope>, MetaDataStore<MetaMessageEnvelope>>();
            services.AddSingleton<IStorageClient, StorageClient>();
            services.AddSingleton<IInboxService, InboxService>();

            services.Configure<BlobStorageOptions>(options =>
            {
                options.BlobConnectionString = config["Volvo:Inbox:BlobConnectionString"];
                options.ContainerName = config["Volvo:Inbox:ContainerName"];
            });

            services.Configure<MetaDataStoreOptions>(options =>
            {
                options.TableConnectionString = config["Volvo:Inbox:TableConnectionString"];
                options.TableName = config["Volvo:Inbox:TableName"];
            });

            services.Configure<InboxRepositoryOptions>(options =>
            {
                options.EarliestNumberOfDaysToRetrieveMessages = config.GetValue<int>("Volvo:Inbox:EarliestNumberOfDaysToRetrieveMessages");
            });

            return services;
        }
    }
}
