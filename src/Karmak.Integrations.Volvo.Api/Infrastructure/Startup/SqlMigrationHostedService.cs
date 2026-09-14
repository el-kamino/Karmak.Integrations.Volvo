using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.Common.Sql.Migrations;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Api.Infrastructure.Startup
{
    /// <summary>
    /// Brings the sql schema up to date before the application begins serving traffic.
    /// </summary>
    public class SqlMigrationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SqlDataLayerOptions _options;
        private readonly ILogger<SqlMigrationHostedService> _logger;

        public SqlMigrationHostedService(
            IServiceProvider serviceProvider,
            IOptions<SqlDataLayerOptions> options,
            ILogger<SqlMigrationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                //Environments without a sql database configured should still start
                _logger.LogWarning("No Volvo:Sql:ConnectionString is configured; skipping sql migrations.");
                return;
            }

            //A failure here is fatal on purpose: running against an unknown schema is worse than not starting
            var runner = _serviceProvider.GetRequiredService<ISqlMigrationRunner>();
            await runner.ApplyPendingScriptsAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
