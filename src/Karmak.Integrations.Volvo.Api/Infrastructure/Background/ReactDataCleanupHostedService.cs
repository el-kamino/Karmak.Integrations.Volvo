using Azure.Messaging.EventGrid.SystemEvents;
using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.React.Persistence.React;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Api.Infrastructure.Background
{
    /// <summary>
    /// Once every 24 hours, at <see cref="ReactDataCleanupOptions.RunAtUtc"/>, deletes the
    /// <c>dbo.ReactDataEntities</c> rows that have aged past the retention window.
    /// </summary>
    public class ReactDataCleanupHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SqlDataLayerOptions _sqlOptions;
        private readonly ReactDataCleanupOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<ReactDataCleanupHostedService> _logger;

        public ReactDataCleanupHostedService(
            IServiceProvider serviceProvider,
            IOptions<SqlDataLayerOptions> sqlOptions,
            IOptions<ReactDataCleanupOptions> options,
            TimeProvider timeProvider,
            ILogger<ReactDataCleanupHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _sqlOptions = sqlOptions.Value;
            _options = options.Value;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        /// <summary>
        /// The next occurrence of <paramref name="runAtUtc"/>, which is today's if it has not passed yet.
        /// Only used to align the first run; subsequent runs step a whole day from the one before.
        /// </summary>
        internal static DateTimeOffset GetNextRunUtc(DateTimeOffset utcNow, TimeSpan runAtUtc)
        {
            var todaysRun = new DateTimeOffset(utcNow.UtcDateTime.Date + runAtUtc, TimeSpan.Zero);
            return todaysRun >= utcNow ? todaysRun : todaysRun.AddDays(1);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Volvo:Sql:Cleanup:Enabled is false; react data cleanup will not run.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_sqlOptions.ConnectionString))
            {
                //Environments without a sql database configured should still start
                _logger.LogWarning("No Volvo:Sql:ConnectionString is configured; skipping react data cleanup.");
                return;
            }

            //Resolved after the guards above: SqlConnectionFactory throws without a connection string
            var repository = _serviceProvider.GetRequiredService<IReactDataRepository>();

            var nextRun = GetNextRunUtc(_timeProvider.GetUtcNow(), _options.RunAtUtc);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "React data cleanup will next run at {NextRunUtc:o}, deleting rows older than {RetentionDays} day(s).",
                    nextRun,
                    _options.RetentionDays);

                try
                {
                    var delay = nextRun - _timeProvider.GetUtcNow();
                    await Task.Delay(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, _timeProvider, stoppingToken);
                    await CleanupAsync(repository, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                //Stepping a whole day from the slot just run, rather than recomputing from the clock,
                //keeps the cadence at 24 hours and stops a fast pass from re-firing the same slot
                nextRun = nextRun.AddDays(1);

                if (nextRun < _timeProvider.GetUtcNow())
                {
                    //A pass that overran its own window; realign on the next clean slot
                    nextRun = GetNextRunUtc(_timeProvider.GetUtcNow(), _options.RunAtUtc);
                }
            }
        }

        private async Task CleanupAsync(IReactDataRepository repository, CancellationToken stoppingToken)
        {
            var cutoffUtc = _timeProvider.GetUtcNow().UtcDateTime - _options.RetentionPeriod;

            //Databases in dev and qa have autopause enabled which sometimes leads to the first (or more) of the data
            //cleanups to fail.  Ping the database a few times to ensure it has started.
            await CheckConnectivityAsync(repository, stoppingToken);

            foreach (var entityType in ReactDataEntityTypes.All)
            {
                try
                {
                    int deleted = await repository.DeleteOlderThanAsync(
                        entityType,
                        cutoffUtc,
                        _options.BatchSize,
                        stoppingToken);

                    _logger.LogInformation(
                        "React data cleanup deleted {DeletedCount} {EntityType} row(s) created before {CutoffUtc:o}.",
                        deleted,
                        entityType,
                        cutoffUtc);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    //One entity type failing must not stop the rest; the next run picks its rows up again
                    _logger.LogError(
                        exception,
                        "React data cleanup failed for {EntityType}; continuing with the remaining entity types.",
                        entityType);
                }
            }
        }

        private async Task CheckConnectivityAsync(IReactDataRepository repository, CancellationToken stoppingToken)
        {
            int pingAttempt = 0;

            while(true)
            {
                try
                {
                    await repository.PingAsync(stoppingToken);
                    return;
                }
                catch (Exception e)
                {
                    if (pingAttempt < 4)
                    {
                        pingAttempt++;

                        var delay = TimeSpan.FromSeconds(30);
                        _logger.LogError(e, $"Ping attempt {pingAttempt} failed. Waiting {delay} to try again.");
                        await Task.Delay(delay, stoppingToken);
                    }
                    else
                    {
                        _logger.LogError(e, $"Failed on last ping attempt ({pingAttempt}). Giving up...");
                        return;
                    }
                }
            }
        }
    }
}
